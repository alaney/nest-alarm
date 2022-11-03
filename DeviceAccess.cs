
using System.Net.Http.Headers;
using Google.Cloud.PubSub.V1;
using Newtonsoft.Json;

namespace nestalarm
{
  public class DeviceAccess
  {
    private string projectId;
    private string accessToken;
    private string refreshToken;
    private string oauthClientSecret;
    private string oauthClientId;
    private HttpClient client;

    public DeviceAccess(string accessToken, string refreshToken, string projectId, string oauthClientSecret, string oauthClientId)
    {
      this.accessToken = accessToken;
      this.refreshToken = refreshToken;
      this.projectId = projectId;
      this.oauthClientId = oauthClientId;
      this.oauthClientSecret = oauthClientSecret;
      this.client = new HttpClient();
    }

    public async Task Authenticate()
    {
      try
      {
        await GetCameras();
      }
      catch (HttpRequestException ex)
      {
        await RefreshAccessToken();
        var cameras = await GetCameras();
        Console.WriteLine("\nException Caught!");
        Console.WriteLine("Message :{0} ", ex.Message);
      }
    }

    public async Task RefreshAccessToken()
    {
      var refreshUri = $"https://www.googleapis.com/oauth2/v4/token?client_id={oauthClientId}&client_secret={oauthClientSecret}&refresh_token={refreshToken}&grant_type=refresh_token";
      try
      {
        StringContent content = new StringContent("");
        using (HttpResponseMessage response = await client.PostAsync(refreshUri, content))
        {
          response.EnsureSuccessStatusCode();
          string responseBody = await response.Content.ReadAsStringAsync();
          dynamic resp = JsonConvert.DeserializeObject(responseBody);

          if (resp?.access_token != null)
          {
            this.accessToken = resp.access_token;
          }
        }
      }
      catch (HttpRequestException ex)
      {
        Console.WriteLine("\nException Caught!");
        Console.WriteLine("Message :{0} ", ex.Message);
      }
    }


    public async Task<bool> CheckForPersonEventAsync(string projectId, string subscriptionId, bool acknowledge)
    {
      SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
      SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName);
      // SubscriberClient runs your message handle function on multiple
      // threads to maximize throughput.
      int personEventCount = 0;
      Task startTask = subscriber.StartAsync((PubsubMessage message, CancellationToken cancel) =>
      {
        string text = System.Text.Encoding.UTF8.GetString(message.Data.ToArray());
        if (text.Contains("sdm.devices.events.CameraPerson.Person"))
        {
          Interlocked.Increment(ref personEventCount);
        }
        return Task.FromResult(acknowledge ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack);
      });
      // Run for 5 seconds.
      await Task.Delay(5000);
      await subscriber.StopAsync(CancellationToken.None);
      // Lets make sure that the start task finished successfully after the call to stop.
      await startTask;
      return personEventCount > 0;
    }

    private async Task<List<Camera>> GetCameras()
    {
      client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      var devicesUri = $"https://smartdevicemanagement.googleapis.com/v1/enterprises/{projectId}/devices";
      using (HttpResponseMessage response = await client.GetAsync(devicesUri))
      {
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        DevicesResponse devicesResp = JsonConvert.DeserializeObject<DevicesResponse>(responseBody);

        if (devicesResp != null && devicesResp.devices != null)
        {
          var devices = devicesResp.devices;
          var cameraDevices = devices.FindAll(c => c.type.Contains("CAMERA"));
          var cameras = cameraDevices.Select(c =>
          {
            string id = ParseDeviceIdFromName(c.name);
            return new Camera(id);
          }).ToList();

          return cameras;
        }

        throw new ApplicationException("The devices response was malformed");
      }
    }

    private async Task TurnOffCamera()
    {

    }

    private string ParseDeviceIdFromName(string name)
    {
      string[] parts = name.Split('/');
      return parts[3];
    }
  }
}