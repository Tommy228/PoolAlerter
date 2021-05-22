using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        /// <returns></returns>
        public Task NotifyErrorsAsync([NotNull] IEnumerable<string> errors, PoolAvailabilityResultContext context);
    }
}