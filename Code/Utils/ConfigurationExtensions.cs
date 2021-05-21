using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PoolAlerter.Code.Utils
{
    internal static class ConfigurationExtensions
    {
        public static IServiceCollection AddBoundConfiguration<T>(
            this IServiceCollection services,
            [NotNull] string path
        ) where T : class, new()
            =>
                services.AddSingleton(provider =>
                {
                    var configuration = provider.GetRequiredService<IConfiguration>();
                    var section = configuration.GetSection(path);
                    var configurationClass = new T();
                    section.Bind(configurationClass);
                    return configurationClass;
                });
    }
}