using JetStreamSubscriberHosted.Server.Extensions;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddNats(sp =>
{
    sp.Servers = new[] { "nats://localhost:4222", "nats://localhost:5222" };
    sp.Name = "BlazorHosted";
    sp.Timeout = 3000;
});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var config = new TypeAdapterConfig();
//var config = TypeAdapterConfig.GlobalSettings;
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
