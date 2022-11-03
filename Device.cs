namespace nestalarm
{
  public class Device
  {
    public string name { get; set; }
    public string type { get; set; }
  }

  public class DevicesResponse
  {
    public List<Device>? devices { get; set; }
  }
}