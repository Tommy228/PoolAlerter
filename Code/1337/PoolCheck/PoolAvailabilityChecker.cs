using System;
using System.Linq;
using FluentResults;
using Functional.Maybe;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Extensions;
using PoolAlerter.Code._1337.Configuration;
using LogLevel = OpenQA.Selenium.LogLevel;

namespace PoolAlerter.Code._1337.PoolCheck
{
    internal class PoolAvailabilityChecker : IPoolAvailabilityChecker
    {
        private readonly _1337Configuration _configuration;
        private readonly ILogger<PoolAvailabilityChecker> _logger;

        public bool IsCheckInProgress { get; private set; }

        public PoolAvailabilityChecker(_1337Configuration configuration, ILogger<PoolAvailabilityChecker> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public (Result<bool> Result, PoolAvailabilityResultContext Context) CheckPoolAvailabilityAsync()
        {
            this.IsCheckInProgress = true;

            _logger.LogDebug("Starting chrome driver");
            
            using var driver = CreateWebDriver();

            var loginResult = this.Login(driver);
            var resultContext = new PoolAvailabilityResultContext
            {
                Screenshot = driver.TakeScreenshot().AsByteArray
            };
            
            if (!loginResult.IsSuccess)
            {
                this.IsCheckInProgress = false;
                return (Result.Fail<bool>(loginResult.Errors.FirstOrDefault()), resultContext);
            }
            
            try
            {
                _logger.LogDebug("Checking pool availability by viewing text");

                var poolText = driver.FindElement(By.CssSelector("#subs-content > div.row > div > p"));
                var isPoolAvailable = !poolText?.Text.TrimStart().StartsWith(
                    "De nouveaux creneaux ouvriront prochainement",
                    StringComparison.InvariantCulture
                ) ?? true;

                return (Result.Ok(isPoolAvailable), resultContext);
            }
            catch (NoSuchElementException)
            {
                return (Result.Ok(true), resultContext);
            }
            catch (Exception)
            {
                return (Result.Fail("Could not determine if a pool is available"), resultContext);
            }
            finally
            {
                this.IsCheckInProgress = false;
            }
        }

        private Result Login(IWebDriver driver)
        {
            var url = _configuration.Url;
            _logger.LogDebug("Navigating to {Url}", url);
            driver.Navigate().GoToUrl(new Uri(url));

            try
            {
                _logger.LogDebug("Writing authentication information and submitting");

                var emailInput = driver.FindElement(By.CssSelector("#user_email"));
                emailInput.SendKeys(_configuration.Email);

                var passwordInput = driver.FindElement(By.CssSelector("#user_password"));
                passwordInput.SendKeys(_configuration.Password);

                var submit =
                    driver.FindElement(By.CssSelector("#new_user > div.form-inputs > div.form-actions > input"));
                submit.Click();

                return Result.Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Authentication failed");
                return Result.Fail("Could not log in");
            }
        }

        private IWebDriver CreateWebDriver()
        {
            var chromeOptions = new ChromeOptions();
            if (_configuration.Webdriver.Headless)
            {
                chromeOptions.AddArgument("headless");
            }

            chromeOptions.AddArgument("--window-size=1920,1080");
            
            foreach (var (logType, logLevelName) in _configuration.Webdriver.LogLevels)
            {
                var isLogLevelValid = Enum.TryParse(typeof(LogLevel), logLevelName, out var logLevel);
                if (!isLogLevelValid) continue;
                chromeOptions.SetLoggingPreference(logType, (LogLevel) logLevel!);
            }   

            return new ChromeDriver(chromeOptions);
        }
    }
}