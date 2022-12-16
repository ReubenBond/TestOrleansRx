using GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

try
{
    using IHost host = await StartSiloAsync();
    Console.WriteLine("\n\n Press Enter to terminate...\n\n");

    var client = host.Services.GetRequiredService<IClusterClient>();

    await DoClientWorkAsync(client);

    await host.WaitForShutdownAsync();

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    return 1;
}

static async Task<IHost> StartSiloAsync()
{
    var builder = new HostBuilder()
        .UseOrleans(silo =>
        {
            silo.UseLocalhostClustering()
                .ConfigureLogging(logging => logging.AddConsole());
        })
        .UseConsoleLifetime();

    var host = builder.Build();
    await host.StartAsync();

    return host;
}

static async Task DoClientWorkAsync(IClusterClient client)
{
    var friend = client.GetGrain<IHello>("Alex");
    var response = await friend.ApplyDot(1000);

    Console.WriteLine($"\n\n{response}\n\n");

    response = await friend.SayHello("Hello from Orleans Clent!");
    Console.WriteLine($"\n\n{response}\n\n");
}