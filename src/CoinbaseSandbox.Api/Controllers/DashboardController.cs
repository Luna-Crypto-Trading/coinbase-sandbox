using Microsoft.AspNetCore.Mvc;

namespace CoinbaseSandbox.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet("websocket-tester")]
    public IActionResult GetWebSocketTester()
    {
        var path = Path.Combine(environment.WebRootPath, "websocket-tester.html");
        if (!System.IO.File.Exists(path))
        {
            return NotFound("WebSocket tester page not found");
        }

        return PhysicalFile(path, "text/html");
    }

    [HttpGet("dashboard")]
    public IActionResult GetDashboard()
    {
        var path = Path.Combine(environment.WebRootPath, "dashboard.html");
        if (!System.IO.File.Exists(path))
        {
            return NotFound("Dashboard page not found");
        }

        return PhysicalFile(path, "text/html");
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return Redirect("/api/dashboard/websocket-tester");
    }
}