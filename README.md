# PoolAlerter

Simple .NET application that monitors the [1337 website](https://candidature.1337.ma/users/sign_in) with candidate credentials to check if a pool is available.

Using Selenium, it will open the a web browser with a given frequency and checks if the message saying pools are unavailable is still there.

Before running it, edit `appsettings.json` to add the following information:
* 1337 credentials
* [Discord token](https://discord.com/developers/docs/topics/oauth2)
* The ids of the users and channels where you want to notify

If needed, it can be ran with Docker.
