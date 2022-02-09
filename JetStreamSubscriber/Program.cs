using JetStreamShared;
using JetStreamShared.Extensions;
using JetStreamSubscriber;
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
        services.AddHostedService<JetStreamSubscribe>();
        services.AddScoped<StreamSpace>();
        services.AddScoped<Consumer>();
        services.AddScoped<Subscribe>();
        services.AddScoped<Helper>();
    });

builder.Build().RunAsync();


//    .AddNats(sp =>
//{
//    sp.Servers = new[] { "nats://localhost:4222", "nats://localhost:5222" };
//    sp.Name = "BlazorHosted";
//    sp.Timeout = 3000;
//});