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

// Configure static files to serve the WebSocket tester
builder.Services.AddDirectoryBrowser();

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
builder.Services.AddScoped<IBacktestService, BacktestService>();
builder.Services.AddScoped<ITechnicalAnalysisService, TechnicalAnalysisService>();

// Add API-specific services
builder.Services.AddScoped<CoinbaseSandbox.Api.Services.IOrderConfigurationParser, CoinbaseSandbox.Api.Services.OrderConfigurationParser>();

// Add infrastructure services
builder.Services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
builder.Services.AddSingleton<INotificationService, NotificationService>();

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

    // Define the security schemes for API key and Bearer token authentication
    c.AddSecurityDefinition("CoinbaseApiKey", new OpenApiSecurityScheme
    {
        Description = "Coinbase API Key authentication using the CB-ACCESS-KEY header",
        Name = "CB-ACCESS-KEY",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Add Security requirements for all operations
    var apiKeyScheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "CoinbaseApiKey"
        },
        In = ParameterLocation.Header
    };

    var bearerScheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { apiKeyScheme, new string[] { } },
        { bearerScheme, new string[] { } }
    });

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure static files and directory browsing for the wwwroot folder
app.UseStaticFiles();

var wwwrootBrowserPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (Directory.Exists(wwwrootBrowserPath))
{
    app.UseDirectoryBrowser(new DirectoryBrowserOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(wwwrootBrowserPath),
        RequestPath = "/browser"
    });
}

// Create wwwroot directory if it doesn't exist
var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

var dashboardSourceFile = Path.Combine(Directory.GetCurrentDirectory(), "src/CoinbaseSandbox.Api/wwwroot/dashboard.html");
var dashboardDestFile = Path.Combine(wwwrootPath, "dashboard.html");
if (System.IO.File.Exists(dashboardSourceFile) && !System.IO.File.Exists(dashboardDestFile))
{
    System.IO.File.Copy(dashboardSourceFile, dashboardDestFile);
}

// Copy WebSocket tester HTML file to wwwroot if it exists
var sourceFile = Path.Combine(Directory.GetCurrentDirectory(), "websocker-tester.html");
var destFile = Path.Combine(wwwrootPath, "websocket-tester.html");
if (System.IO.File.Exists(sourceFile) && !System.IO.File.Exists(destFile))
{
    System.IO.File.Copy(sourceFile, destFile);
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