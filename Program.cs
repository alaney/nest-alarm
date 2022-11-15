using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Configuration;

namespace nestalarm
{
  internal class Program
  {
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
      while (true)
      {
        // and the phone has not been answered
        if (WithinTimeRange(start, end))
        {
          await TurnCamerasOn(homeFoyer);

          // an event occurred and someone answered the phone
          if (state.PhoneAnswered && state.MessageSid != "")
          {
            ShouldRestart(state.MessageSid);
          }
          else
          {
            bool personEvent = await deviceAccess.CheckForPersonEventAsync(true);

            if (personEvent)
            {
              state.AnsweredPhoneNumber = await CallPhones(appOptions.Phones, appOptions.Twilio);
              if (state.PhoneAnswered)
              {
                state.MessageSid = SendText(state.AnsweredPhoneNumber, appOptions.Twilio.Number);
              }

              // what if no one answered?
              // call again?
              // or nothing?
            }
          }
        }
        else
        {
          await TurnCamerasOff(homeFoyer);
          // continue to ack events so they don't pile up
          bool personEvent = await deviceAccess.CheckForPersonEventAsync(true);
        }
      }
    }

    private static async Task TurnCamerasOn(GoogleHomeFoyer homeFoyer)
    {
      if (!state.CamerasOn)
      {
        await homeFoyer.TurnOnAllCameras();
        state.CamerasOn = true;
      }
    }

    private static async Task TurnCamerasOff(GoogleHomeFoyer homeFoyer)
    {
      if (state.CamerasOn)
      {
        await homeFoyer.TurnOffAllCameras();
        state.CamerasOn = false;
      }
    }

    private static bool WithinTimeRange(TimeSpan start, TimeSpan end)
    {
      TimeSpan midnight = new TimeSpan(24, 0, 0);
      TimeSpan midnight2 = new TimeSpan(0, 0, 0);

      TimeSpan now = DateTime.Now.TimeOfDay;

      return ((now >= start) && (now <= midnight)) || ((now >= midnight2) && (now <= end));
    }

    private static void ShouldRestart(string messageSid)
    {
      var messages = MessageResource.Read(limit: 20);
      // find message by SID.
      // and see if there's one right after it with test "RESTART"
      Console.WriteLine(messages);
    }

    private static string SendText(string toPhone, string fromPhone)
    {
      var message = MessageResource.Create(
          body: "The Nest Alarm system is paused. To restart, reply RESTART.",
          from: new Twilio.Types.PhoneNumber(fromPhone),
          to: new Twilio.Types.PhoneNumber(toPhone)
      );

      return message.Sid;
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