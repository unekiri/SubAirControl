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
            var builder = Host.CreateDefaultBuilder(args);

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // IServiceCollectonに設定を登録する
            builder.ConfigureServices((ContextBoundObject, services) =>
            {
                // IconfigurationをDIコンテナに追加
                services.AddSingleton<IConfiguration>(config);
                services.AddSingleton<ExeSubscriber>();
            });

            var host = builder.Build();
            var exeSubscriber = host.Services.GetRequiredService<ExeSubscriber>();

            await exeSubscriber.Run();
        }
    }
}