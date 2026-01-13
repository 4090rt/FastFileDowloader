// See https://aka.ms/new-console-template for more information
using FileDowloader.MainDirectory;
using FileDowloader.Request;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

var service = new ServiceCollection();

service.AddLogging(build =>
{ 
    build.AddConsole();
    build.SetMinimumLevel(LogLevel.Information);
});

service.AddHttpClient("httpclient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36\"");


}).AddTransientHttpErrorPolicy(policy =>
policy.WaitAndRetryAsync(3, retry =>
TimeSpan.FromSeconds(Math.Pow(2, retry))));

service.AddScoped<MainClass>();
service.AddScoped<Request>();
var serviceprovide = service.BuildServiceProvider();

using var scope = serviceprovide.CreateScope();
var main = scope.ServiceProvider.GetRequiredService<MainClass>();
//await main.MutexMethod();