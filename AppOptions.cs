namespace nestalarm
{
  public class AppOptions
  {
    public TwilioOptions Twilio { get; set; } = new TwilioOptions();
    public DeviceAccessOptions DeviceAccess { get; set; } = new DeviceAccessOptions();
    public string[] Phones { get; set; } = new List<string>().ToArray();
    public HomeFoyerHeaderOptions HomeFoyerRequestHeaders { get; set; } = new HomeFoyerHeaderOptions();
    public List<Camera> HomeFoyerCameras { get; set; } = new List<Camera>();

  }

  public class HomeFoyerHeaderOptions
  {
    public string Authorization { get; set; } = String.Empty;
    public string Cookie { get; set; } = String.Empty;
    public string XGoogleApiKey { get; set; } = String.Empty;
  }

  public class TwilioOptions
  {
    public string AccountSid { get; set; } = String.Empty;
    public string AuthToken { get; set; } = String.Empty;
    public string Number { get; set; } = String.Empty;
  }

  public class DeviceAccessOptions
  {
    public string DeviceAccessProjectId { get; set; } = String.Empty;
    public string OAuthClientId { get; set; } = String.Empty;
    public string OAuthClientSecret { get; set; } = String.Empty;
    public string AccessToken { get; set; } = String.Empty;
    public string RefreshToken { get; set; } = String.Empty;
    public string SubscriptionId { get; set; } = String.Empty;
    public string ProjectId { get; set; } = String.Empty;
  }
}