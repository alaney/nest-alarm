// Find your Account SID and Auth Token at twilio.com/console
// and set the environment variables. See http://twil.io/secure
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static void Main(string[] args)
    {
       var configuration =  new ConfigurationBuilder()
     .SetBasePath(Directory.GetCurrentDirectory())
     .AddJsonFile($"appsettings.json");
            
var config = configuration.Build();

        var accountSid = config["TWILIO_ACCOUNT_SID"];
        string authToken =  config["TWILIO_AUTH_TOKEN"];

        TwilioClient.Init(accountSid, authToken);

        var call = CallResource.Create(
            url: new Uri("http://demo.twilio.com/docs/voice.xml"),
            to: new Twilio.Types.PhoneNumber("+12565585887"),
            from: new Twilio.Types.PhoneNumber("+13609681398")
        );
    }
}