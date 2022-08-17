using Grpc.Core;
using GrpcDemo.V1;
using Microsoft.AspNetCore.Mvc;

namespace GrpcDemo.Client.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class StreamingFromServerController : ControllerBase
{
    private readonly Example.ExampleClient _client;
    private readonly ILogger<StreamingFromServerController> _logger;

    public StreamingFromServerController(Example.ExampleClient client, ILogger<StreamingFromServerController> logger)
    {
        _client = client;
        _logger = logger;
    }

    [HttpGet(Name = "StreamingFromServer")]
    public async Task<string> Get()
    {
        var call = _client.StreamingFromServer(new ExampleRequest { IsDescending = false, PageSize = 0, });

        var messages = new List<string>();
        await foreach (var response in call.ResponseStream.ReadAllAsync())
        {
            messages.Add(response.Message);
            _logger.LogInformation("Example Message: " + response.Message);
        }

        return string.Join(", ", messages);
    }
}
