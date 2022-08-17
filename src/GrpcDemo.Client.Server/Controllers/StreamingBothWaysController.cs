using Grpc.Core;
using GrpcDemo.V1;
using Microsoft.AspNetCore.Mvc;

namespace GrpcDemo.Client.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class StreamingBothWaysController : ControllerBase
{
    private readonly Example.ExampleClient _client;
    private readonly ILogger<StreamingBothWaysController> _logger;

    public StreamingBothWaysController(Example.ExampleClient client, ILogger<StreamingBothWaysController> logger)
    {
        _client = client;
        _logger = logger;
    }

    [HttpGet(Name = "StreamingBothWays")]
    public async Task<string> Get()
    {
        var call = _client.StreamingBothWays();

        _logger.LogInformation("Starting background task to receive messages");
        var messages = new List<string>();
        var readTask = Task.Run(async () =>
        {
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                _logger.LogInformation("Received a message from the server: " + response.Message);
                messages.Add(response.Message);
            }
        });

        _logger.LogInformation("Starting to send messages three times");

        for (int i = 0; i < 3; i++)
        {
            await call.RequestStream.WriteAsync(new ExampleRequest { PageIndex = i, IsDescending = false, PageSize = 0, });
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        _logger.LogInformation("Disconnecting");
        await call.RequestStream.CompleteAsync();
        await readTask;

        return messages.Count > 0 ? string.Join(", ", messages) : "No messages received";
    }
}
