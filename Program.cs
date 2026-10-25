using Fleck;
using System.Collections.Concurrent;

var clients = new ConcurrentDictionary<Guid, IWebSocketConnection>();
var server = new WebSocketServer("ws://0.0.0.0:8181");

server.Start(ws =>
{
    ws.OnOpen = () =>
    {
        clients[ws.ConnectionInfo.Id] = ws;   // add the client
        Console.WriteLine("Client connected");
    };
    ws.OnClose = () =>
    {
        clients.TryRemove(ws.ConnectionInfo.Id, out _);   // remove the client
        Console.WriteLine("Client disconnected");
    };
    ws.OnMessage = message =>
    {
        Console.WriteLine($"Received: {message}");
        ws.Send($"Echo: {message}");
    };
});

_ = Task.Run(() =>
{
    while (true)
    {
        var line = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(line))
            Broadcast(line);
    }
});

void Broadcast(string message)
{
    foreach (var client in clients.Values)
        client.Send(message);
}

WebApplication.CreateBuilder(args).Build().Run();