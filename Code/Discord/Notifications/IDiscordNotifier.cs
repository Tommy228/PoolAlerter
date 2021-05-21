using System.Threading.Tasks;

namespace PoolAlerter.Code.Discord.Notifications
{
    internal interface IDiscordNotifier
    {
        public Task NotifyPoolAvailabilityAsync(bool isAvailable);
    }
}