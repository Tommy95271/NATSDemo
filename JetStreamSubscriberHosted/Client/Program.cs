using JetStreamSubscriberHosted.Client;
using JetStreamSubscriberHosted.Client.Services;
using JetStreamSubscriberHosted.Client.Services.Implements;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddHttpClient("NATS", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://localhost:7181/");
});

builder.Services.AddScoped<IStreamService, StreamService>();

builder.Services.AddTelerikBlazor();

await builder.Build().RunAsync();
