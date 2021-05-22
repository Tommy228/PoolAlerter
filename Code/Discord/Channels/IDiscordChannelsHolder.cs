using Discord.WebSocket;
using Functional.Maybe;

namespace PoolAlerter.Code.Discord.Channels
{
    public interface IDiscordChannelsHolder
    {
        public Maybe<SocketTextChannel> NotificationsChannel { get; }
        
        public Maybe<SocketTextChannel> ErrorsChannel { get; }
        
        public Maybe<SocketTextChannel> HeartbeatsChannel { get; }
    }
}