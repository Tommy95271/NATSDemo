using JetStreamShared;
using JetStreamShared.Extensions;
using JetStreamPublisher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddNats(sp =>
        {
            sp.Servers = new[] { "nats://localhost:4222", "nats://localhost:5222" };
            sp.Name = "BlazorHosted";
            sp.Timeout = 3000;
        });
        services.AddHostedService<JetStreamPublish>();
        services.AddScoped<StreamSpace>();
        services.AddScoped<Consumer>();
        services.AddScoped<Publish>();
        services.AddScoped<Helper>();
    });

builder.Build().RunAsync();