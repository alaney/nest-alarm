namespace nestalarm
{
  public class GoogleHomeFoyer
  {
    private readonly string api = "https://googlehomefoyer-pa.clients6.google.com/$rpc/google.internal.home.foyer.v1.HomeControlService/UpdateTraits";
    private readonly string cookie;
    private readonly string authorization;
    private readonly string googleApiKey;
    private readonly Camera[] cameras;

    public GoogleHomeFoyer(string authorization, string cookie, string googleApiKey, Camera[] cameras)
    {
      this.authorization = authorization;
      this.cookie = cookie;
      this.googleApiKey = googleApiKey;
      this.cameras = cameras;
    }

    public async Task TurnOnAllCameras()
    {
      for (int i = 0; i < cameras.Length; i++)
      {
        Camera cam = cameras[i];
        await TurnOnCamera(cam);
      }
    }

    public async Task TurnOffAllCameras()
    {
      for (int i = 0; i < cameras.Length; i++)
      {
        Camera cam = cameras[i];
        await TurnOffCamera(cam);
      }
    }

    private async Task TurnOffCamera(Camera camera)
    {
      string content = $"[[[[\"{camera.id}\",[\"nest-home-assistant-prod\",\"{camera.deviceId}\"]],[[\"onOff\",[[\"onOff\",[null,null,null,false]]]]]]]]";
      await SendHomeFoyerRequest(content);
    }

    private async Task TurnOnCamera(Camera camera)
    {
      string content = $"[[[[\"{camera.id}\",[\"nest-home-assistant-prod\",\"{camera.deviceId}\"]],[[\"onOff\",[[\"onOff\",[null,null,null,true]]]]]]]]";
      await SendHomeFoyerRequest(content);
    }

    private async Task<HttpResponseMessage> SendHomeFoyerRequest(string content)
    {
      HttpResponseMessage response;
      using (var client = new HttpClient())
      {
        using (var request = new HttpRequestMessage(HttpMethod.Post, api))
        {
          request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(authorization);
          request.Headers.Add("content-type", "application/json+protobuf");
          request.Headers.Add("origin", "https://home.google.com");
          request.Headers.Add("cookie", cookie);
          request.Headers.Add("x-goog-api-key", googleApiKey);
          request.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
          response = await client.SendAsync(request);
        }
      }
      return response;
    }
  }
}