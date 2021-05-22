using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PoolAlerter.Code.Discord.Configuration;

namespace PoolAlerter.Code.Discord.Startup
{
    public class DiscordStartupService : IHostedService
    {
        private readonly DiscordConfiguration _configuration;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly ILogger<DiscordStartupService> _logger;

        public DiscordStartupService(
            DiscordConfiguration configuration,
            DiscordSocketClient discordSocketClient,
            ILogger<DiscordStartupService> logger
        )
        {
            _configuration = configuration;
            _discordSocketClient = discordSocketClient;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Connecting to Discord");

            await _discordSocketClient.LoginAsync(TokenType.Bot, _configuration.Token);
            await _discordSocketClient.StartAsync();

            _logger.LogInformation("Connected to Discord");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordSocketClient.StopAsync();
        }
    }
}