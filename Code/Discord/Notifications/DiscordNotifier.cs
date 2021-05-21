using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using PoolAlerter.Code.Discord.Configuration;

namespace PoolAlerter.Code.Discord.Notifications
{
    internal class DiscordNotifier : IDiscordNotifier
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly DiscordConfiguration _discordConfiguration;

        private SocketGuild Guild => _discordSocketClient.GetGuild(_discordConfiguration.ServerId);

        private SocketTextChannel NotificationChannel =>
            Guild?.GetTextChannel(_discordConfiguration.Channels.NotificationChannelId);

        public DiscordNotifier(DiscordSocketClient discordSocketClient, DiscordConfiguration discordConfiguration)
        {
            _discordSocketClient = discordSocketClient ?? throw new ArgumentNullException(nameof(discordSocketClient));
            _discordConfiguration =
                discordConfiguration ?? throw new ArgumentNullException(nameof(discordConfiguration));
        }

        public async Task NotifyPoolAvailabilityAsync(bool isAvailable)
        {
            await NotificationChannel.SendMessageAsync($"Just checked and available {isAvailable}");
        }
    }
}