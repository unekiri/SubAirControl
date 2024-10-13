using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubAirControl.Subscribe;

namespace SubAirControl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                services.AddSingleton<IConfiguration>(config);
                services.AddSingleton<Subscriber>();
                services.AddSingleton<ExeSubscriber>();
            });

            var host = builder.Build();

            var exeSubscriber = host.Services.GetRequiredService<ExeSubscriber>();
            await exeSubscriber.Run();
        }
    }
}