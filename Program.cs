﻿using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Configuration;

namespace nestalarm
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json");

            var config = configuration.Build();
            var accessToken = config["ACCESS_TOKEN"];
            var refreshToken = config["REFRESH_TOKEN"];
            var projectId = config["PROJECT_ID"];
            var oauthClientId = config["OAUTH2_CLIENT_ID"];
            var oauthClientSecret = config["OAUTH2_CLIENT_SECRET"];
            var subscriptionId = config["SUBSCRIPTION_ID"];

            if (String.IsNullOrEmpty(accessToken) || String.IsNullOrEmpty(refreshToken) || String.IsNullOrEmpty(projectId)) {
                throw new ApplicationException("Must provde a access token and refresh token");
            }

            DeviceAccess deviceAccess = new DeviceAccess(accessToken, refreshToken, projectId, oauthClientSecret, oauthClientId);
            await deviceAccess.Authenticate();

            await CheckEvents(deviceAccess, projectId, subscriptionId);
        }

        private static async Task CheckEvents(DeviceAccess deviceAccess, string projectId, string subscriptionId)
        {
            while(true) 
            {
                TimeSpan start = new TimeSpan(20, 0, 0);
                TimeSpan midnight = new TimeSpan(24, 0, 0);
                TimeSpan midnight2 = new TimeSpan(0, 0, 0);
                TimeSpan end = new TimeSpan(5, 45, 0);
                TimeSpan now = DateTime.Now.TimeOfDay;

                if(((now >= start) && (now < midnight)) || (now >= midnight2 && now <= end))
                {
                    await deviceAccess.PullMessagesAsync(projectId, subscriptionId, true);
                    await Task.Delay(5000);
                }
            }
        }

        private static void MakeCall(IConfigurationRoot config)
        {
            var accountSid = config["TWILIO_ACCOUNT_SID"];
            string authToken = config["TWILIO_AUTH_TOKEN"];

            TwilioClient.Init(accountSid, authToken);

            var call = CallResource.Create(
                url: new Uri("http://demo.twilio.com/docs/voice.xml"),
                to: new Twilio.Types.PhoneNumber("+12565585887"),
                from: new Twilio.Types.PhoneNumber("+13609681398")
            );
        }
    }
}