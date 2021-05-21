using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PoolAlerter.Code._1337.Configuration;
using PoolAlerter.Code._1337.PoolCheck;
using PoolAlerter.Code.Discord.Configuration;
using PoolAlerter.Code.Discord.Notifications;
using PoolAlerter.Code.Discord.Startup;
using PoolAlerter.Code.Monitor;
using PoolAlerter.Code.Monitor.Configuration;
using PoolAlerter.Code.Utils;

await Host
    .CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostBuilderContext, config) =>
        config
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile($"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json", true)
    )
    .ConfigureLogging(logging =>
        logging
            .ClearProviders()
            .AddConsole()
    )
    .ConfigureServices(services =>
    {
        services
            .AddSingleton(provider => new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = provider.GetRequiredService<DiscordConfiguration>().ParseLogSeverity(),
                MessageCacheSize = 1000
            }))
            .AddBoundConfiguration<_1337Configuration>("1337")
            .AddBoundConfiguration<DiscordConfiguration>("Discord")
            .AddBoundConfiguration<MonitorConfiguration>("Monitor")
            .AddTransient<IPoolAvailabilityChecker, PoolAvailabilityChecker>()
            .AddTransient<IDiscordNotifier, DiscordNotifier>()
            .AddHostedService<DiscordStartupService>()
            .AddHostedService<PoolMonitor>();
    })
    .RunConsoleAsync();
    
    