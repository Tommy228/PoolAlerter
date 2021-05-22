using System.Collections.Generic;
using System.Threading.Tasks;
using PoolAlerter.Code._1337.PoolCheck;

namespace PoolAlerter.Code.Discord.Notifications
{
    internal interface IDiscordNotifier
    {
        public Task NotifyPoolAvailabilityAsync(bool isAvailable, PoolAvailabilityResultContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errors"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task NotifyErrorsAsync(IEnumerable<string> errors, PoolAvailabilityResultContext context);

        public Task SendHeartbeat();
    }
}