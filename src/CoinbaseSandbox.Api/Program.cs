// Program.cs
using CoinbaseSandbox.Api.WebSockets;
using CoinbaseSandbox.Application.Services;
using CoinbaseSandbox.Domain.Repositories;
using CoinbaseSandbox.Domain.Services;
using CoinbaseSandbox.Infrastructure.Data;
using CoinbaseSandbox.Infrastructure.External;
using CoinbaseSandbox.Infrastructure.Repositories;
using CoinbaseSandbox.Infrastructure.Services;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebSocketManager = CoinbaseSandbox.Api.WebSockets.WebSocketManager;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON options for API responses
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

// Add services to the container.
builder.Services.AddControllers();

// Add WebSocket support
builder.Services.AddSingleton<WebSocketManager>();

// Add repositories (in-memory for sandbox)
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddSingleton<IWalletRepository, InMemoryWalletRepository>();
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton<IPriceRepository, InMemoryPriceRepository>();

// Add application services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPriceService, PriceService>();

// Add infrastructure services
builder.Services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();

// Add HttpClient for real Coinbase API passthrough
builder.Services.AddHttpClient<CoinbaseApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.coinbase.com");
});

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Coinbase Sandbox API",
        Version = "v1",
        Description = "A sandbox API that mimics the Coinbase Advanced Trade API but only mocks order execution, account balances, and wallet operations"
    });
    
    // Define the security scheme for API key authentication
    c.AddSecurityDefinition("CoinbaseApiKey", new OpenApiSecurityScheme
    {
        Description = "Coinbase API Key authentication using the CB-ACCESS-KEY header",
        Name = "CB-ACCESS-KEY",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });
    
    // Add Security requirement for all operations
    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "CoinbaseApiKey"
        },
        In = ParameterLocation.Header
    };
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { scheme, new string[] { } }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure WebSockets
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120),
    ReceiveBufferSize = 4 * 1024  // 4KB buffer
});

// Add WebSocket middleware
app.UseMiddleware<WebSocketMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Seed data when application starts
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    var productRepository = services.GetRequiredService<IProductRepository>();
    var walletRepository = services.GetRequiredService<IWalletRepository>();
    var priceRepository = services.GetRequiredService<IPriceRepository>();
    
    await SeedData.InitializeAsync(productRepository, walletRepository, priceRepository);
}

app.Run();

public partial class Program;