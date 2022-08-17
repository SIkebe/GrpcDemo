using Grpc.Core;
using GrpcDemo;

namespace GrpcDemo.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Saying hello to {Name}", request.Name);

        context.ResponseTrailers.Add("Trailer-Key", "Trailer-Value");

        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}