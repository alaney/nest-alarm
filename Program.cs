using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Configuration;

namespace nestalarm
{
  internal class Program
  {
    private static bool checking = true;
    private static async Task Main(string[] args)
    {
      var configuration = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile($"appsettings.json");

      var config = configuration.Build();
      var appOptions = new AppOptions();
      config.Bind(appOptions);

      GoogleHomeFoyer homeFoyer = new GoogleHomeFoyer(appOptions.HomeFoyerRequestHeaders, appOptions.HomeFoyerCameras);
      DeviceAccess deviceAccess = new DeviceAccess(appOptions.DeviceAccess);
      await deviceAccess.Authenticate();

      string messageSid = "";
      string answeredPhone = "";
      while (true)
      {
        TimeSpan start = new TimeSpan(0, 0, 0);
        TimeSpan midnight = new TimeSpan(24, 0, 0);
        TimeSpan midnight2 = new TimeSpan(0, 0, 0);
        TimeSpan end = new TimeSpan(24, 0, 0);
        TimeSpan now = DateTime.Now.TimeOfDay;

        if (((now >= start) && (now < midnight)) || (now >= midnight2 && now <= end))
        {
          checking = true;
        }

        if (checking)
        {
          await WaitForPersonEvent(deviceAccess);
          answeredPhone = await CallPhones(appOptions.Phones, appOptions.Twilio);
          if (answeredPhone != "")
          {
            checking = false;
            messageSid = SendText(answeredPhone, appOptions.Twilio.Number);
          }
        }
        else
        {
          await WaitForPersonEvent(deviceAccess);
          CheckTextResponse(messageSid);
          await Task.Delay(60 * 1000);
        }
      }
    }

    private static void CheckTextResponse(string messageSid)
    {
      var messages = MessageResource.Read(limit: 20);
      Console.WriteLine(messages);
    }

    private static string SendText(string toPhone, string fromPhone)
    {
      var message = MessageResource.Create(
          body: "Join Earth's mightiest heroes. Like Kevin Bacon.",
          from: new Twilio.Types.PhoneNumber(fromPhone),
          to: new Twilio.Types.PhoneNumber(toPhone)
      );

      return message.Sid;
    }

    private static async Task WaitForPersonEvent(DeviceAccess deviceAccess)
    {
      while (true)
      {
        bool personEvent = await deviceAccess.CheckForPersonEventAsync(true);
        if (personEvent)
        {
          break;
        }
        await Task.Delay(5000);
      }
    }

    // Calls a list of phone numbers in order and returns the phone number that a human answered, if a human answers.
    // otherwise returns an empty string.
    private static async Task<string> CallPhones(string[] phones, TwilioOptions twilioOptions)
    {
      TwilioClient.Init(twilioOptions.AccountSid, twilioOptions.AuthToken);
      for (int i = 0; i < phones.Length; i++)
      {
        string phone = phones[i];
        // Call each number twice to bypass "do not disturb" mode.
        for (int j = 0; j < 2; j++)
        {
          string callSid = MakeCall(phone, twilioOptions.Number);
          while (true)
          {
            CallResource call = CallResource.Fetch(callSid);
            bool callCompleted = call.Status == CallResource.StatusEnum.Completed;
            bool answeredByHuman = call.AnsweredBy == "human";
            if (answeredByHuman)
            {
              return phone;
            }
            else if (callCompleted)
            {
              break;
            }
            await Task.Delay(1000);
          }
        }
      }

      return "";
    }

    private static string MakeCall(string toPhone, string fromPhone)
    {
      var call = CallResource.Create(
          url: new Uri("http://demo.twilio.com/docs/voice.xml"),
          to: new Twilio.Types.PhoneNumber(toPhone),
          from: new Twilio.Types.PhoneNumber(fromPhone),
          machineDetection: "Enable"
      );
      return call.Sid;
    }
  }
}