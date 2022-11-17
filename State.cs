namespace nestalarm
{
  public class State
  {
    private string _answeredPhoneNumber = string.Empty;
    private bool _smsMessageSent = false;
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
        _camerasOn = value;
      }
    }

    public bool PhoneAnswered
    {
      get { return _phoneAnswered; }
    }

    public bool SmsMessageSent
    {
      get { return _smsMessageSent; }
      set { _smsMessageSent = value; }
    }

    public void Reset()
    {
      _answeredPhoneNumber = "";
      _smsMessageSent = false;
      _phoneAnswered = false;
      _camerasOn = true;
    }
  }
}