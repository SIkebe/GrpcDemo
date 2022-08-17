using GrpcDemo.V1;
using Microsoft.AspNetCore.Mvc;

namespace GrpcDemo.Client.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class StreamingFromClientController : ControllerBase
{
    private readonly Example.ExampleClient _client;
    private readonly ILogger<StreamingFromClientController> _logger;

    public StreamingFromClientController(Example.ExampleClient client, ILogger<StreamingFromClientController> logger)
    {
        _client = client;
        _logger = logger;
    }

    [HttpGet(Name = "StreamingFromClient")]
    public async Task<string> Get()
    {
        var call = _client.StreamingFromClient();

        for (int i = 0; i < 3; i++)
        {
            _logger.LogInformation($"ExampleRequest: {i}");
            await call.RequestStream.WriteAsync(new ExampleRequest { PageIndex = i, IsDescending = false, PageSize = 0, });
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        await call.RequestStream.CompleteAsync();

        var response = await call;
        return response.Message;
    }
}
