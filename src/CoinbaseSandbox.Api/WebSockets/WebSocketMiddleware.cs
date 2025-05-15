namespace CoinbaseSandbox.Api.WebSockets;

using System.Net.WebSockets;
using System.Text;

/// <summary>
/// Middleware to handle WebSocket connections
/// </summary>
public class WebSocketMiddleware(
    RequestDelegate next,
    WebSocketManager webSocketManager,
    ILogger<WebSocketMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/ws")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                var socketId = Guid.NewGuid().ToString();

                webSocketManager.AddSocket(socketId, socket);

                await HandleSocketAsync(socketId, socket);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        else
        {
            await next(context);
        }
    }

    private async Task HandleSocketAsync(string socketId, WebSocket socket)
    {
        var buffer = new byte[4096];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await webSocketManager.ProcessMessageAsync(socketId, message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    webSocketManager.RemoveSocket(socketId);

                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Connection closed by client",
                        CancellationToken.None);

                    break;
                }
            }
        }
        catch (WebSocketException ex)
        {
            logger.LogWarning(ex, "WebSocket error for {SocketId}", socketId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling WebSocket for {SocketId}", socketId);
        }
        finally
        {
            webSocketManager.RemoveSocket(socketId);

            if (socket.State != WebSocketState.Closed)
            {
                try
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.EndpointUnavailable,
                        "Server error",
                        CancellationToken.None);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }
    }
}