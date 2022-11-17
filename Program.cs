using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Configuration;

namespace nestalarm
{
  internal class Program
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private static State state = new State();
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
      // Clear any events
      await deviceAccess.CheckForPersonEventAsync(true);

      TimeSpan start = new TimeSpan(10, 0, 0);
      TimeSpan end = new TimeSpan(9, 59, 59);
      TimeSpan fiveMinutes = new TimeSpan(0, 5, 0);
      TimeSpan fifteenMinutes = new TimeSpan(0, 15, 0);

      while (true)
      {
        // takes 5 seconds
        bool personEvent = await deviceAccess.CheckForPersonEventAsync(true);

        if (WithinTimeRange(start, end))
        {
          if (!state.CamerasOn)
          {
            Logger.Info("Within time range. Turning cameras on.");
            await TurnCamerasOn(homeFoyer);
            state.Reset();
          }

          // an event occurred and someone answered the phone
          if (state.PhoneAnswered && state.SmsMessageSent)
          {
            await Task.Delay(fiveMinutes);
            Logger.Info("Checking text response");
            if (ShouldRestart())
            {
              Logger.Info("Received RESTART text.");
              state.Reset();
            }
          }
          else if (personEvent)
          {
            Logger.Info("Person event occurred");
            state.AnsweredPhoneNumber = await CallPhones(appOptions.Phones, appOptions.Twilio);

            if (state.PhoneAnswered)
            {
              Logger.Info("Sending text");
              SendText(state.AnsweredPhoneNumber, appOptions.Twilio.Number);
              state.SmsMessageSent = true;
            }
          }
        }
        else
        {
          if (state.CamerasOn)
          {
            Logger.Info("Within time range. Turning cameras off.");
            await TurnCamerasOff(homeFoyer);
          }
          // no reason to loop often outside time range
          await Task.Delay(fifteenMinutes);
        }
      }
    }

    private static async Task TurnCamerasOn(GoogleHomeFoyer homeFoyer)
    {
      await homeFoyer.TurnOnAllCameras();
      state.CamerasOn = true;
    }

    private static async Task TurnCamerasOff(GoogleHomeFoyer homeFoyer)
    {
      await homeFoyer.TurnOffAllCameras();
      state.CamerasOn = false;
    }

    private static bool WithinTimeRange(TimeSpan start, TimeSpan end)
    {
      TimeSpan midnight = new TimeSpan(24, 0, 0);
      TimeSpan midnight2 = new TimeSpan(0, 0, 0);

      TimeSpan now = DateTime.Now.TimeOfDay;

      return ((now >= start) && (now <= midnight)) || ((now >= midnight2) && (now <= end));
    }

    private static bool ShouldRestart()
    {
      var messages = MessageResource.Read(limit: 1);
      var message = messages.FirstOrDefault();
      Logger.Info(message?.ToString());
      return (message != null && message.Body == "RESTART");
    }

    private static void SendText(string toPhone, string fromPhone)
    {
      MessageResource.Create(
          body: "The Nest Alarm system is paused. To restart, reply RESTART.",
          from: new Twilio.Types.PhoneNumber(fromPhone),
          to: new Twilio.Types.PhoneNumber(toPhone)
      );
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
          Logger.Info("Call made to " + phone);
          while (true)
          {
            CallResource call = CallResource.Fetch(callSid);
            bool callCompleted = call.Status == CallResource.StatusEnum.Completed;
            Logger.Info("Call completed: " + callCompleted);
            bool answeredByHuman = call.AnsweredBy == "human";
            Logger.Info("Call answered: " + answeredByHuman);
            if (answeredByHuman)
            {
              return phone;
            }
            else if (callCompleted)
            {
              break;
            }
            await Task.Delay(2000);
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