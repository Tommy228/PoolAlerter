using System;
using System.Linq;
using FluentResults;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PoolAlerter.Code._1337.Configuration;

namespace PoolAlerter.Code._1337.PoolCheck
{
    internal class PoolAvailabilityChecker : IPoolAvailabilityChecker
    {
        private readonly _1337Configuration _configuration;
        private readonly ILogger<PoolAvailabilityChecker> _logger;

        public bool IsCheckInProgress { get; private set; }
        
        public PoolAvailabilityChecker(_1337Configuration configuration, ILogger<PoolAvailabilityChecker> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Result<bool> CheckPoolAvailabilityAsync()
        {
            _logger.LogDebug("Starting chrome driver");
            
            this.IsCheckInProgress = true;
            
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");
            using var driver = new ChromeDriver(chromeOptions);

            var loginResult = this.Login(driver);
            if (!loginResult.IsSuccess)
            {
                this.IsCheckInProgress = false;
                return Result.Fail<bool>(loginResult.Errors.FirstOrDefault());
            }

            try
            {
                _logger.LogDebug("Checking pool availability by viewing text");

                var poolText = driver.FindElement(By.CssSelector("#subs-content > div.row > div > p"));
                var isPoolAvailable = !poolText?.Text.TrimStart().StartsWith(
                    "De nouveaux creneaux ouvriront prochainement",
                    StringComparison.InvariantCulture
                ) ?? true;

                return Result.Ok(isPoolAvailable);
            }
            catch (NoSuchElementException)
            {
                return Result.Ok(true);
            }
            catch (Exception)
            {
                return Result.Fail("Could not determine if a pool is available");
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
    }
}