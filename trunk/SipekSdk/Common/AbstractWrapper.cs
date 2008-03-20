using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{
  // event methods prototypes
  public delegate void DCallStateChanged(int callId, int callState, string info);
  public delegate void DCallIncoming(int callId, string number, string info);
  public delegate void DAccountStateChanged(int accountId, int accState);
  public delegate void DMessageReceived(string from, string text);
  public delegate void DBuddyStatusChanged(int buddyId, int status, string text);
  public delegate void DDtmfDigitReceived(int callId, int digit);
  public delegate void DCallNotification(int callId, ECallNotification notFlag, string text);
  public delegate void DMessageWaitingNotification(int mwi, string text);

   
  /// <summary>
  /// Non-call oriented VoIP interface. 
  /// Handles events invoked by VoIP stack.
  /// Methods called by user side.
  /// </summary>
  public abstract class IVoipProxy
  {
    #region Events

    /// <summary>
    /// Events exposed to user layers. A protected virtual method added because 
    /// derived classes cannot invoke events directly.
    /// </summary>

    public event DCallStateChanged CallStateChanged;
    protected void BaseCallStateChanged(int callId, int callState, string info)
    {
      if (null != CallStateChanged) CallStateChanged(callId, callState, info);
    }

    public event DCallIncoming CallIncoming;
    protected void BaseIncomingCall(int callId, string number, string info)
    {
      if (null != CallIncoming) CallIncoming(callId, number, info);
    }

    public event DCallNotification CallNotification;
    protected void BaseCallNotification(int callId, ECallNotification notifFlag, string text)
    {
      if (null != CallNotification) CallNotification(callId, notifFlag, text);
    }

    public event DAccountStateChanged AccountStateChanged;
    protected void BaseAccountStateChanged(int accountId, int accState)
    {
      if (null != AccountStateChanged) AccountStateChanged(accountId, accState);
    }

    public event DMessageReceived MessageReceived;
    protected void BaseMessageReceived(string from, string text)
    {
      if (null != MessageReceived) MessageReceived(from, text);
    }

    public event DBuddyStatusChanged BuddyStatusChanged;
    protected void BaseBuddyStatusChanged(int buddyId, int status, string text)
    {
      if (null != BuddyStatusChanged) BuddyStatusChanged(buddyId, status, text);
    }

    public event DDtmfDigitReceived DtmfDigitReceived;
    protected void BaseDtmfDigitReceived(int callId, int digit)
    {
      if (null != DtmfDigitReceived) DtmfDigitReceived(callId, digit);
    }

    public event DMessageWaitingNotification MessageWaitingIndication;
    protected void BaseMessageWaitingIndication(int mwi, string text)
    {
      if (null != MessageWaitingIndication) MessageWaitingIndication(mwi, text);
    }

    #endregion events

    #region Properties

    public abstract bool IsInitialized
    {
      get;
      set;
    }
    #endregion 

    #region Public methods

    public abstract int initialize();
    public abstract int shutdown();

    public abstract int registerAccounts();

    public abstract int addBuddy(string ident);

    public abstract int delBuddy(int buddyId);

    public abstract int sendMessage(string dest, string message);

    public abstract int setStatus(int accId, EUserStatus presence_state);

    public abstract void setCodecPrioroty(string item, int p);

    public abstract int getNoOfCodecs();

    public abstract string getCodec(int i);
    
    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  public interface ICallProxyInterface
  {
    #region Properties
    int SessionId
    { get; set; }
    #endregion

    #region Public Methods

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

    #endregion
  }


  #region Null Pattern

  /// <summary>
  /// 
  /// </summary>
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


  /// <summary>
  /// 
  /// </summary>
  public class NullVoipProxy : IVoipProxy
  {
    #region ICommonProxyInterface Members

    public override int initialize()
    {
      return 1;
    }

    public override int shutdown()
    {
      return 1;
    }

    public override int registerAccounts()
    {
      return 1;
    }

    public override int addBuddy(string ident)
    {
      return 1;
    }

    public override int delBuddy(int buddyId)
    {
      return 1;
    }

    public override int sendMessage(string dest, string message)
    {
      return 1;
    }

    public override int setStatus(int accId, EUserStatus presence_state)
    {
      return 1;
    }

    public override void setCodecPrioroty(string item, int p)
    {
    }
    public override int getNoOfCodecs() { return 0; }

    public override string getCodec(int i) { return ""; }

    #endregion

    public override bool IsInitialized
    {
      get
      {
        throw new Exception("The method or operation is not implemented.");
      }
      set
      {
        throw new Exception("The method or operation is not implemented.");
      }
    }
  }
  #endregion

}
