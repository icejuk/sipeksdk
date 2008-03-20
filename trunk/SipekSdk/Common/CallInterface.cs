using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{

  /// <summary>
  /// 
  /// </summary>
  public interface ICallProxyInterface
  {
    int SessionId
    { get; set; }

    int makeCall(string dialedNo, int accountId);

    bool endCall();

    bool alerted();

    bool acceptCall();

    bool holdCall();

    bool retrieveCall();

    bool xferCall(string number);

    bool xferCallSession(int partnersession);

    bool threePtyCall(int partnersession);

    //bool serviceRequest(EServiceCodes code, int session);
    bool serviceRequest(int code, string dest);

    bool dialDtmf(string digits, int mode);
  }

  #region Null Pattern
  public class NullCallProxy : ICallProxyInterface
  {
    #region ICallProxyInterface Members

    public int makeCall(string dialedNo, int accountId)
    {
      return 1;
    }

    public bool endCall()
    {
      return false;
    }

    public bool alerted()
    {
      return false;
    }

    public bool acceptCall()
    {
      return false;
    }

    public bool holdCall()
    {
      return false;
    }

    public bool retrieveCall()
    {
      return false;
    }

    public bool xferCall(string number)
    {
      return false;
    }

    public bool xferCallSession(int session)
    {
      return false;
    }

    public bool threePtyCall(int session)
    {
      return false;
    }

    public bool serviceRequest(int code, string dest)
    {
      return false;
    }

    public bool dialDtmf(string digits, int mode)
    {
      return false;
    }

    #endregion

    #region ICallProxyInterface Members

    public int SessionId
    {
      get { return 0; }
      set { ; }
    }

    #endregion
  }

  #endregion

}
