using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using GrpcDemo.V1;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#if false

builder.Services.AddSingleton<ResolverFactory>(
    sp => new DnsResolverFactory(refreshInterval: TimeSpan.FromSeconds(30)));

// gRPC client factory integration in .NET
// https://docs.microsoft.com/en-us/aspnet/core/grpc/clientfactory?view=aspnetcore-6.0
builder.Services.AddGrpcClient<Example.ExampleClient>((serviceProvider, options) =>
{
    options.Address = new Uri("dns:///my-example-host");
    
    var retryCount = 2;
    if (!string.IsNullOrEmpty(builder.Configuration["GrpcRetryCount"]))
    {
        retryCount = int.Parse(builder.Configuration["GrpcRetryCount"]);
    }

    var defaultMethodConfig = new MethodConfig
    {
        Names = { MethodName.Default },
        RetryPolicy = new RetryPolicy
        {
            MaxAttempts = retryCount,
            InitialBackoff = TimeSpan.FromSeconds(1),
            MaxBackoff = TimeSpan.FromSeconds(5),
            BackoffMultiplier = 1.5,
            RetryableStatusCodes = { StatusCode.Unavailable }
        }
    };
    options.ChannelOptionsActions.Add(options =>
    {
        options.Credentials = ChannelCredentials.Insecure;
        options.ServiceConfig = new ServiceConfig
        {
            LoadBalancingConfigs = { new RoundRobinConfig() },
            MethodConfigs = { defaultMethodConfig },
        };
    });
});

#else

// gRPC client-side load balancing
// https://docs.microsoft.com/en-us/aspnet/core/grpc/loadbalancing?view=aspnetcore-6.0#staticresolverfactory
builder.Services.AddSingleton<ResolverFactory>(new StaticResolverFactory(_ => new[]
{
    new BalancerAddress("localhost", 7195),
    new BalancerAddress("localhost", 7194),
}));

// gRPC client factory integration in .NET
// https://docs.microsoft.com/en-us/aspnet/core/grpc/clientfactory?view=aspnetcore-6.0
builder.Services.AddGrpcClient<Example.ExampleClient>((serviceProvider, options) =>
{
    options.Address = new Uri("static:///localhost");

    var retryCount = 2;
    if (!string.IsNullOrEmpty(builder.Configuration["GrpcRetryCount"]))
    {
        retryCount = int.Parse(builder.Configuration["GrpcRetryCount"]);
    }

    var defaultMethodConfig = new MethodConfig
    {
        Names = { MethodName.Default },
        RetryPolicy = new RetryPolicy
        {
            MaxAttempts = retryCount,
            InitialBackoff = TimeSpan.FromSeconds(1),
            MaxBackoff = TimeSpan.FromSeconds(5),
            BackoffMultiplier = 1.5,
            RetryableStatusCodes = { StatusCode.Unavailable }
        }
    };

    options.ChannelOptionsActions.Add(options =>
    {
        options.ServiceProvider = serviceProvider;
        options.Credentials = ChannelCredentials.SecureSsl;
        options.ServiceConfig = new ServiceConfig
        {
            LoadBalancingConfigs = { new RoundRobinConfig() },
            MethodConfigs = { defaultMethodConfig },
        };
    });
});

#endif

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
