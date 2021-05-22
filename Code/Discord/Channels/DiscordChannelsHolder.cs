using Discord.WebSocket;
using Functional.Maybe;
using PoolAlerter.Code.Discord.Configuration;

namespace PoolAlerter.Code.Discord.Channels
{
    internal class DiscordChannelsHolder : IDiscordChannelsHolder
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly DiscordConfiguration _discordConfiguration;

        private Maybe<SocketGuild> Guild => _discordSocketClient.GetGuild(_discordConfiguration.ServerId).ToMaybe();

        public Maybe<SocketTextChannel> NotificationsChannel =>
            GetChannel(_discordConfiguration.Channels.NotificationChannelId);

        public Maybe<SocketTextChannel> ErrorsChannel =>
            GetChannel(_discordConfiguration.Channels.ErrorsChannelId);

        public Maybe<SocketTextChannel> HeartbeatsChannel =>
            GetChannel(_discordConfiguration.Channels.HeartbeatChannelId);

        public DiscordChannelsHolder(DiscordSocketClient discordSocketClient, DiscordConfiguration discordConfiguration)
        {
            _discordSocketClient = discordSocketClient;
            _discordConfiguration = discordConfiguration;
        }

        private Maybe<SocketTextChannel> GetChannel(ulong channelId) =>
            Guild.SelectMaybe(guild => guild.GetTextChannel(channelId).ToMaybe());
    }
}