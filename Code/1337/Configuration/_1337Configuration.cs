using System.Collections.Generic;

namespace PoolAlerter.Code._1337.Configuration
{
    internal record _1337Configuration
    {
        public string Url { get; init; }
        
        public string Email { get; init; }
        
        public string Password { get; init; }
        
        public _1337ConfigurationWebdriverSettings Webdriver { get; init; }
    }

    internal record _1337ConfigurationWebdriverSettings
    {
        public bool Headless { get; init; }
        
        public IDictionary<string, string> LogLevels { get; init; }
    }
}