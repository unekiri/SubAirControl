using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubAirControl.Helpers;
using SubAirControl.Services.Subscribe;

namespace SubAirControl
{
    class Programs
    {
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            // ctrl + cが押された時に、CancellationTokenを機能させる
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
            };

            var builder = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                services.AddSingleton<IConfiguration>(config);
                services.AddSingleton<IConnectionHelper, MqttConnectionHelper>();
                services.AddSingleton<AtFirstExeSubscribe>();
            });

            var host = builder.Build();

            var atFirstExeSubscribe = host.Services.GetRequiredService<AtFirstExeSubscribe>();
            await atFirstExeSubscribe.Run(cts.Token);
        }
    }
}