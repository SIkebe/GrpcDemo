using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcDemo.V1;

namespace GrpcDemo.Services;

public class ExampleServiceV1 : Example.ExampleBase
{
    private readonly ILogger<ExampleServiceV1> _logger;

    public ExampleServiceV1(ILogger<ExampleServiceV1> logger)
    {
        _logger = logger;
    }

    public override async Task<ExampleResponse> UnaryCall(ExampleRequest request, ServerCallContext context)
    {
        var userAgent = context.RequestHeaders.Get("user-agent");
        var response = new ExampleResponse()
        {
            Message = $"userAgent is {userAgent?.Value}",
            Start = Timestamp.FromDateTime(DateTime.UtcNow),
            Duration = Duration.FromTimeSpan(TimeSpan.FromSeconds(1)),
            Payload = ByteString.CopyFrom(new byte[] { 1, 2, 3, }),
            Roles = { "admin", "user", },
            Attributes = { { "key1", "value1" }, { "key2", "value2" }, },
            Detail = Any.Pack(new Person { FirstName = "Sergei", LastName = "Rachmaninoff" }),
            Success = new Success { Message = "Success!" },
            Data = Value.Parser.ParseJson("""
                {
                    "enabled": true,
                    "metadata": [ "value1", "value2" ]
                }
                """)
        };

        return await Task.FromResult(response);
    }

    public override async Task StreamingFromServer(ExampleRequest request, IServerStreamWriter<ExampleResponse> responseStream, ServerCallContext context)
    {
        for (var i = 0; i < 5; i++)
        {
            await responseStream.WriteAsync(new ExampleResponse { Message = i.ToString() });
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

#if false
    public override async Task StreamingFromServer(ExampleRequest request, IServerStreamWriter<ExampleResponse> responseStream, ServerCallContext context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            await responseStream.WriteAsync(new ExampleResponse());
            await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
        }
    }
#endif

    public override async Task<ExampleResponse> StreamingFromClient(IAsyncStreamReader<ExampleRequest> requestStream, ServerCallContext context)
    {
        await foreach (var message in requestStream.ReadAllAsync())
        {
            _logger.LogInformation($"PageIndex is {message.PageIndex}");
            _logger.LogInformation($"PageSize is {message.PageSize}");
            _logger.LogInformation($"IsDescending is {message.IsDescending}");
        }

        return new ExampleResponse { Message = "StreamingFromClient is called!" };
    }

    public override async Task StreamingBothWays(
        IAsyncStreamReader<ExampleRequest> requestStream,
        IServerStreamWriter<ExampleResponse> responseStream,
        ServerCallContext context)
    {
        await foreach (var message in requestStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(new ExampleResponse { Message = $"PageIndex is {message.PageIndex}" });
        }
    }

#if false
    public override async Task StreamingBothWays(
        IAsyncStreamReader<ExampleRequest> requestStream,
        IServerStreamWriter<ExampleResponse> responseStream,
        ServerCallContext context)
    {
        // Read requests in a background task.
        var readTask = Task.Run(async () =>
        {
            await foreach (var message in requestStream.ReadAllAsync())
            {
                // Process request.
            }
        });

        // Send responses until the client signals that it is complete.
        while (!readTask.IsCompleted)
        {
            await responseStream.WriteAsync(new ExampleResponse());
            await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
        }
    }
#endif
}
