using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PoolAlerter.Code.Discord.Notifications
{
    internal interface IDiscordNotifier
    {
        public Task NotifyPoolAvailabilityAsync(bool isAvailable);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Task NotifyErrorsAsync([NotNull] IEnumerable<string> errors);
    }
}