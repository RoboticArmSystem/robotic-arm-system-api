using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using RoboticArmSystem.Core.Extensions;
using RoboticArmSystem.Core.Hubs;
using RoboticArmSystem.Core.Response;
using RoboticArmSystem.Core.Service.Config;
using RoboticArmSystem.Core.Service.MQTTBackgroundService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var environmentName = builder.Environment.EnvironmentName;
var configRoot = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
    .Build();

builder.Configuration.AddConfiguration(configRoot);


var CORS_POLICY = "AllowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CORS_POLICY,
        policy =>
        {
            policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddServices();
builder.Services.AddTransient<ResponseService>();
builder.Services.AddSingleton(new MQTTBackgroundService(builder.Services.BuildServiceProvider().GetRequiredService<ConfigService>()));
builder.Services.AddSingleton<MqttFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin 
    .AllowCredentials());

//app.UseRouting();

app.UseAuthorization();

app.MapHub<SignalRAsyncHub>("/localHub");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
