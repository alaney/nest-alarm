using System.Net;
using System.Net.Http.Headers;

namespace nestalarm
{
  public class GoogleHomeFoyer
  {
    private readonly string api = "https://googlehomefoyer-pa.clients6.google.com/$rpc/google.internal.home.foyer.v1.HomeControlService/UpdateTraits";
    private readonly string cookie;
    private readonly string authorization;
    private readonly string googleApiKey;
    private readonly List<Camera> cameras;

    public GoogleHomeFoyer(HomeFoyerHeaderOptions options, List<Camera> cameras)
    {
      this.authorization = options.Authorization;
      this.cookie = options.Cookie;
      this.googleApiKey = options.XGoogleApiKey;
      this.cameras = cameras;
    }

    public async Task<Boolean> TurnOnAllCameras()
    {
      Boolean successful = true;

      for (int i = 0; i < cameras.Count; i++)
      {
        Camera cam = cameras[i];
        var success = await TurnOnCamera(cam);
        if (!success)
        {
          successful = false;
        }
      }

      return successful;
    }

    public async Task<Boolean> TurnOffAllCameras()
    {
      Boolean successful = true;

      for (int i = 0; i < cameras.Count; i++)
      {
        Camera cam = cameras[i];
        var success = await TurnOffCamera(cam);
        if (!success)
        {
          successful = false;
        }
      }

      return successful;
    }

    private async Task<Boolean> TurnOffCamera(Camera camera)
    {
      string content = $"[[[[\"{camera.id}\",[\"nest-home-assistant-prod\",\"{camera.deviceId}\"]],[[\"onOff\",[[\"onOff\",[null,null,null,false]]]]]]]]";
      var response = await SendHomeFoyerRequest(content);
      return response.StatusCode == HttpStatusCode.OK;
    }

    private async Task<Boolean> TurnOnCamera(Camera camera)
    {
      string content = $"[[[[\"{camera.id}\",[\"nest-home-assistant-prod\",\"{camera.deviceId}\"]],[[\"onOff\",[[\"onOff\",[null,null,null,true]]]]]]]]";
      var response = await SendHomeFoyerRequest(content);
      return response.StatusCode == HttpStatusCode.OK;
    }

    private async Task<HttpResponseMessage> SendHomeFoyerRequest(string content)
    {
      HttpResponseMessage response;
      using (var client = new HttpClient())
      {
        using (var request = new HttpRequestMessage(HttpMethod.Post, api))
        {
          request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json+protobuf"));
          request.Headers.Add("authorization", authorization);
          // request.Headers.Authorization = new AuthenticationHeaderValue(authorization);
          // request.Headers.Add("content-type", "application/json+protobuf");
          request.Headers.Add("origin", "https://home.google.com");
          request.Headers.Add("cookie", cookie);
          request.Headers.Add("x-goog-api-key", googleApiKey);
          request.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json+protobuf");
          response = await client.SendAsync(request);
        }
      }
      return response;
    }
  }
}