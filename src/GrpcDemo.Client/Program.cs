using System.Text.Json;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo;
using GrpcDemo.V1;

using var channel = GrpcChannel.ForAddress("https://localhost:7194");

var greeterClient = new Greeter.GreeterClient(channel);

var greeterCall = greeterClient.SayHelloAsync(
    new HelloRequest { Name = "Greeter Client" },
    deadline: DateTime.UtcNow.AddSeconds(10));

try
{
    var greeterResponse = await greeterCall.ResponseAsync;
    Console.WriteLine("Greeting: " + greeterResponse.Message);
}
catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
{
    Console.WriteLine("timeout");    
}

var trailers = greeterCall.GetTrailers();
foreach (var trailer in trailers)
{
    Console.WriteLine($"Trailer: Key={trailer.Key}, Value={trailer.Value}");
}

var exampleClient = new Example.ExampleClient(channel);
var exampleReply = await exampleClient.UnaryCallAsync(new ExampleRequest { IsDescending = false, PageSize = 0, });

Console.WriteLine("Example Message: " + exampleReply.Message);
Console.WriteLine("Example Start: " + exampleReply.Start);
Console.WriteLine("Example Duration: " + exampleReply.Duration);
Console.WriteLine("Example Payload[0]: " + exampleReply.Payload.Span[0]);
Console.WriteLine("Example Payload[1]: " + exampleReply.Payload.Span[1]);
Console.WriteLine("Example Payload[2]: " + exampleReply.Payload.Span[2]);

foreach (var role in exampleReply.Roles)
{
    Console.WriteLine("Example Role: " + role);
}

foreach (var attribute in exampleReply.Attributes)
{
    Console.WriteLine($"Example Attribute: Key={attribute.Key}, Value={attribute.Value}");
}

if (exampleReply.Detail.Is(Person.Descriptor))
{
    var person = exampleReply.Detail.Unpack<Person>();
    Console.WriteLine($"Example Detail: FirstName={person.FirstName}, LastName={person.LastName}");
}

switch (exampleReply.ResultCase)
{
    case ExampleResponse.ResultOneofCase.Success:
        Console.WriteLine("Example Oneof: " + exampleReply.Success.Message);
        break;

    case ExampleResponse.ResultOneofCase.Error:
        Console.WriteLine("Example Oneof: " + exampleReply.Error.Message);
        break;

    default:
        throw new ArgumentException("Unexpected result.");
}

var json = JsonFormatter.Default.Format(exampleReply.Data);
var document = JsonDocument.Parse(json);
Console.WriteLine("Example Data: " + document.RootElement);
