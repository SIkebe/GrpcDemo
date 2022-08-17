using GrpcDemo.V1;
using Microsoft.AspNetCore.Mvc;

namespace GrpcDemo.Client.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class UnaryCallController : ControllerBase
{
    private readonly Example.ExampleClient _client;
    private readonly ILogger<UnaryCallController> _logger;

    public UnaryCallController(Example.ExampleClient client, ILogger<UnaryCallController> logger)
    {
        _client = client;
        _logger = logger;
    }

    [HttpGet(Name = "UnaryCall")]
    public async Task<string> Get()
    {
        var reply = await _client.UnaryCallAsync(new ExampleRequest { IsDescending = false, PageSize = 0, });
        _logger.LogInformation("Example Message: " + reply.Message);
        return reply.Message;
    }
}
