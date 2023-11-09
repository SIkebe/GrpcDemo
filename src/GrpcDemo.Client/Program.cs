using Grpc.Net.Client;
using GrpcDemo;

using var channel = GrpcChannel.ForAddress("https://grpcdemo20231109214059.azurewebsites.net");

var greeterClient = new Greeter.GreeterClient(channel);

var greeterCall = greeterClient.SayHelloAsync(
    new HelloRequest { Name = "Greeter Client" },
    deadline: DateTime.UtcNow.AddSeconds(10));

var greeterResponse = await greeterCall.ResponseAsync;
Console.WriteLine("Greeting: " + greeterResponse.Message);
