namespace nestalarm
{
  public class State
  {
    private string _answeredPhoneNumber = string.Empty;
    private string _messageSid = string.Empty;
    private bool _phoneAnswered = false;
    private bool _camerasOn = false;

    public string AnsweredPhoneNumber
    {
      get
      {
        return _answeredPhoneNumber;
      }

      set
      {
        _phoneAnswered = !String.IsNullOrEmpty(value);
        _answeredPhoneNumber = value;
      }
    }

    public bool CamerasOn
    {
      get
      {
        return _camerasOn;
      }

      set
      {
        _answeredPhoneNumber = "";
        _messageSid = "";
        _phoneAnswered = false;
        _camerasOn = value;
      }
    }

    public bool PhoneAnswered
    {
      get { return _phoneAnswered; }
    }

    public string MessageSid
    {
      get { return _messageSid; }
      set { _messageSid = value; }
    }
  }
}