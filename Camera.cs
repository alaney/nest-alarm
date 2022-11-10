namespace nestalarm
{
  public class Camera
  {
    public string id { get; set; }
    public string deviceId { get; set; }
    public string name { get; set; }

    public Camera(string id, string deviceId, string name)
    {
      this.id = id;
      this.deviceId = deviceId;
      this.name = name;
    }
  }
}