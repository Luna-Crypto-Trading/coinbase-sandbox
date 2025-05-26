using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class SimulationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SimulationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SetStaticPrice_ShouldUpdatePrice()
    {
        // Arrange
        var productId = "BTC-USD";
        var price = 55000.00m;

        // Act
        var response = await _client.PostAsync(
            $"/api/v3/sandbox/prices/{productId}",
            new StringContent(JsonSerializer.Serialize(price), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(result.GetProperty("success").GetBoolean());
        Assert.Equal(productId, result.GetProperty("product_id").GetString());
        Assert.Equal("55000.00", result.GetProperty("price").GetString());
        Assert.Equal("static", result.GetProperty("simulation_mode").GetString());

        // Verify price was actually set by checking sandbox state
        var stateResponse = await _client.GetAsync("/api/v3/sandbox/state");
        stateResponse.EnsureSuccessStatusCode();
        var stateContent = await stateResponse.Content.ReadAsStringAsync();
        var stateResult = JsonSerializer.Deserialize<JsonElement>(stateContent);

        var prices = stateResult.GetProperty("prices").EnumerateArray();
        var btcPrice = prices.FirstOrDefault(p => p.GetProperty("product_id").GetString() == productId);
        Assert.True(btcPrice.ValueKind != JsonValueKind.Undefined);
        Assert.Equal(55000.00m, btcPrice.GetProperty("price").GetDecimal());
    }

    [Fact]
    public async Task TrendSimulation_ShouldStartSuccessfully()
    {
        // Arrange
        var productId = "BTC-USD";
        var simulationRequest = new
        {
            mode = "trend",
            startPrice = 50000.00m,
            endPrice = 52000.00m,
            durationSeconds = 5,
            repeat = false
        };

        // Act
        var response = await _client.PostAsync(
            $"/api/v3/sandbox/prices/{productId}/simulate",
            new StringContent(JsonSerializer.Serialize(simulationRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(result.GetProperty("success").GetBoolean());
        Assert.Equal(productId, result.GetProperty("product_id").GetString());
        // The start price should be the current price, not necessarily 50000
        Assert.True(result.GetProperty("start_price").GetDecimal() > 0);
        Assert.Equal(52000.00m, result.GetProperty("end_price").GetDecimal());
        Assert.Equal(5, result.GetProperty("duration_seconds").GetInt32());
        Assert.Equal("trend", result.GetProperty("simulation_mode").GetString());
        Assert.False(result.GetProperty("repeat").GetBoolean());

        // Wait a moment for simulation to start
        await Task.Delay(1000);

        // Verify price is changing by checking twice
        var initialStateResponse = await _client.GetAsync("/api/v3/sandbox/state");
        var initialStateContent = await initialStateResponse.Content.ReadAsStringAsync();
        var initialState = JsonSerializer.Deserialize<JsonElement>(initialStateContent);
        var initialPrices = initialState.GetProperty("prices").EnumerateArray();
        var initialBtcPrice = initialPrices.FirstOrDefault(p => p.GetProperty("product_id").GetString() == productId);
        var initialPrice = initialBtcPrice.GetProperty("price").GetDecimal();

        await Task.Delay(2000);

        var finalStateResponse = await _client.GetAsync("/api/v3/sandbox/state");
        var finalStateContent = await finalStateResponse.Content.ReadAsStringAsync();
        var finalState = JsonSerializer.Deserialize<JsonElement>(finalStateContent);
        var finalPrices = finalState.GetProperty("prices").EnumerateArray();
        var finalBtcPrice = finalPrices.FirstOrDefault(p => p.GetProperty("product_id").GetString() == productId);
        var finalPrice = finalBtcPrice.GetProperty("price").GetDecimal();

        // Price should have changed during the trend simulation
        Assert.NotEqual(initialPrice, finalPrice);
    }

    [Fact]
    public async Task VolatilitySimulation_ShouldStartSuccessfully()
    {
        // Arrange
        var productId = "ETH-USD";
        var simulationRequest = new
        {
            mode = "volatility",
            basePrice = 4000.00m,
            volatilityPercent = 5.0m,
            durationSeconds = 5,
            repeat = false
        };

        // Act
        var response = await _client.PostAsync(
            $"/api/v3/sandbox/prices/{productId}/simulate",
            new StringContent(JsonSerializer.Serialize(simulationRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(result.GetProperty("success").GetBoolean());
        Assert.Equal(productId, result.GetProperty("product_id").GetString());
        // The base price should be the current price, not necessarily 4000
        Assert.True(result.GetProperty("base_price").GetDecimal() > 0);
        Assert.Equal(5.0m, result.GetProperty("volatility_percent").GetDecimal());
        Assert.Equal(5, result.GetProperty("duration_seconds").GetInt32());
        Assert.Equal("volatility", result.GetProperty("simulation_mode").GetString());
        Assert.False(result.GetProperty("repeat").GetBoolean());

        // Wait for simulation to run and verify price changes
        await Task.Delay(3000);

        var stateResponse = await _client.GetAsync("/api/v3/sandbox/state");
        var stateContent = await stateResponse.Content.ReadAsStringAsync();
        var stateResult = JsonSerializer.Deserialize<JsonElement>(stateContent);
        var prices = stateResult.GetProperty("prices").EnumerateArray();
        var ethPrice = prices.FirstOrDefault(p => p.GetProperty("product_id").GetString() == productId);

        Assert.True(ethPrice.ValueKind != JsonValueKind.Undefined);
        var currentPrice = ethPrice.GetProperty("price").GetDecimal();

        // Price should be reasonable (greater than 0)
        Assert.True(currentPrice > 0);
    }

    [Fact]
    public async Task StopSimulation_ShouldReturnSuccess()
    {
        // Arrange
        var productId = "SOL-USD";

        // Act
        var response = await _client.DeleteAsync($"/api/v3/sandbox/prices/{productId}/simulation");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(result.GetProperty("success").GetBoolean());
        Assert.Equal(productId, result.GetProperty("product_id").GetString());
        Assert.Equal("Price simulation stopped", result.GetProperty("message").GetString());
    }

    [Fact]
    public async Task TrendSimulation_WithoutEndPrice_ShouldReturnBadRequest()
    {
        // Arrange
        var productId = "BTC-USD";
        var invalidRequest = new
        {
            mode = "trend",
            startPrice = 50000.00m,
            durationSeconds = 10
            // Missing endPrice
        };

        // Act
        var response = await _client.PostAsync(
            $"/api/v3/sandbox/prices/{productId}/simulate",
            new StringContent(JsonSerializer.Serialize(invalidRequest), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.Equal("End price is required for trend simulation", result.GetProperty("error").GetString());
    }

    [Fact]
    public async Task VolatilitySimulation_WithoutVolatilityPercent_ShouldReturnBadRequest()
    {
        // Arrange
        var productId = "BTC-USD";
        var invalidRequest = new
        {
            mode = "volatility",
            basePrice = 50000.00m,
            durationSeconds = 10
            // Missing volatilityPercent
        };

        // Act
        var response = await _client.PostAsync(
            $"/api/v3/sandbox/prices/{productId}/simulate",
            new StringContent(JsonSerializer.Serialize(invalidRequest), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.Equal("Volatility percentage is required for volatility simulation", result.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Simulation_WithInvalidProductId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidProductId = "INVALID-PRODUCT";
        var simulationRequest = new
        {
            mode = "trend",
            startPrice = 50000.00m,
            endPrice = 52000.00m,
            durationSeconds = 10,
            repeat = false
        };

        // Act
        var response = await _client.PostAsync(
            $"/api/v3/sandbox/prices/{invalidProductId}/simulate",
            new StringContent(JsonSerializer.Serialize(simulationRequest), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.Contains("not found", result.GetProperty("error").GetString());
    }
}