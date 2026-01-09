using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;

var builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder.UseLocalhostClustering()
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "dev";
                options.ServiceId = "OrleansSignalRService";
            })
            .ConfigureLogging(logging => logging.AddConsole());
    });

using var host = builder.Build();
await host.RunAsync();
