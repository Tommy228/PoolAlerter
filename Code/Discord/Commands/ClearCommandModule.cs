using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PoolAlerter.Code.Discord.Channels;

namespace PoolAlerter.Code.Discord.Commands
{
    public class ClearCommandModule : ModuleBase<SocketCommandContext>
    {
        private readonly IDiscordChannelsHolder _discordChannelsHolder;

        public ClearCommandModule(IDiscordChannelsHolder discordChannelsHolder)
        {
            _discordChannelsHolder = discordChannelsHolder;
        }

        [Command("clear")]
        public async Task ClearAsync()
        {
            const int count = 100;
            
            var channelsToClear = new[]
            {
                _discordChannelsHolder.ErrorsChannel,
                _discordChannelsHolder.HeartbeatsChannel,
                _discordChannelsHolder.NotificationsChannel
            }.Where(channel => channel.HasValue).Select(channel => channel.Value);

            foreach (var channel in channelsToClear)
            {
                await ClearChannel(channel, count);
            }

            await ReplyAsync($"Deleted the {count} last messages");
        }

        private static async Task ClearChannel(SocketTextChannel channel, int count)
        {
            var messages = await channel.GetMessagesAsync(count).FlattenAsync();
            await channel.DeleteMessagesAsync(messages);
        }
    }
}