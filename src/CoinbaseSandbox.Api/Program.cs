// Program.cs
using CoinbaseSandbox.Application.Services;
using CoinbaseSandbox.Domain.Repositories;
using CoinbaseSandbox.Domain.Services;
using CoinbaseSandbox.Infrastructure.Data;
using CoinbaseSandbox.Infrastructure.External;
using CoinbaseSandbox.Infrastructure.Repositories;
using CoinbaseSandbox.Infrastructure.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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

// Add HttpClient for external APIs
builder.Services.AddHttpClient<ICoinbaseAdvancedTradeClient, MockCoinbaseAdvancedTradeClient>(client =>
{
    // Configure the client for sandbox use
    client.BaseAddress = new Uri("https://api-public.sandbox.pro.coinbase.com");
});

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Coinbase Sandbox API",
        Version = "v1",
        Description = "A sandbox API for testing Coinbase Advanced Trade integration"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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