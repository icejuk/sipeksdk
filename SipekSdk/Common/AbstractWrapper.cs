using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{
  ///
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
  /// Non-call oriented VoIP interface defines events invoked by VoIP stack and 
  /// API consisting methods invoked by user.
  /// </summary>
  public abstract class IVoipProxy
  {
    #region Events

    /// <summary>
    /// Events exposed to user layers. A protected virtual method added because 
    /// derived classes cannot invoke events directly.
    /// </summary>
    

    /// <summary>
    /// CallStateChanged event trigger by VoIP stack when call state changed
    /// </summary>
    public event DCallStateChanged CallStateChanged;
    protected void BaseCallStateChanged(int callId, int callState, string info)
    {
      if (null != CallStateChanged) CallStateChanged(callId, callState, info);
    }
    /// <summary>
    /// CallIncoming event triggered by VoIP stack when new incoming call arrived
    /// </summary>
    public event DCallIncoming CallIncoming;
    protected void BaseIncomingCall(int callId, string number, string info)
    {
      if (null != CallIncoming) CallIncoming(callId, number, info);
    }
    /// <summary>
    /// CallNotification event trigger by VoIP stack when call notification arrived
    /// </summary>
    public event DCallNotification CallNotification;
    protected void BaseCallNotification(int callId, ECallNotification notifFlag, string text)
    {
      if (null != CallNotification) CallNotification(callId, notifFlag, text);
    }
    /// <summary>
    /// AccountStateChanged event trigger by VoIP stack when registration state changed
    /// </summary>
    public event DAccountStateChanged AccountStateChanged;
    protected void BaseAccountStateChanged(int accountId, int accState)
    {
      if (null != AccountStateChanged) AccountStateChanged(accountId, accState);
    }
    /// <summary>
    /// MessageReceived event trigger by VoIP stack when instant message arrived
    /// </summary>
    public event DMessageReceived MessageReceived;
    protected void BaseMessageReceived(string from, string text)
    {
      if (null != MessageReceived) MessageReceived(from, text);
    }
    /// <summary>
    /// BuddyStatusChanged event trigger by VoIP stack when buddy status changed
    /// </summary>
    public event DBuddyStatusChanged BuddyStatusChanged;
    protected void BaseBuddyStatusChanged(int buddyId, int status, string text)
    {
      if (null != BuddyStatusChanged) BuddyStatusChanged(buddyId, status, text);
    }
    /// <summary>
    /// DtmfDigitReceived event trigger by VoIP stack when DTMF is detected 
    /// </summary>
    public event DDtmfDigitReceived DtmfDigitReceived;
    protected void BaseDtmfDigitReceived(int callId, int digit)
    {
      if (null != DtmfDigitReceived) DtmfDigitReceived(callId, digit);
    }
    /// <summary>
    /// MessageWaitingIndication event trigger by VoIP stack when MWI indication arrived 
    /// </summary>
    public event DMessageWaitingNotification MessageWaitingIndication;
    protected void BaseMessageWaitingIndication(int mwi, string text)
    {
      if (null != MessageWaitingIndication) MessageWaitingIndication(mwi, text);
    }

    #endregion events

    #region Properties
    
    private IConfiguratorInterface _config = new NullConfigurator();
    public IConfiguratorInterface Config
    {
      set { _config = value; }
      get { return _config; }
    }

    /// <summary>
    /// Flag indicating stack initialization status
    /// </summary>
    public abstract bool IsInitialized
    {
      get;
      set;
    }
    #endregion 

    #region Public methods
    /// <summary>
    /// Initialize VoIP stack
    /// </summary>
    /// <returns></returns>
    public abstract int initialize();

    /// <summary>
    /// Shutdown VoIP stack
    /// </summary>
    /// <returns></returns>
    public virtual int shutdown()
    {
      AccountStateChanged = null;
      MessageReceived = null;
      BuddyStatusChanged = null;
      DtmfDigitReceived = null;
      MessageWaitingIndication = null;
      CallNotification = null;
      CallIncoming = null;
      CallStateChanged = null;
      return 1;
    }

    /// <summary>
    /// Register all configured accounts
    /// </summary>
    /// <returns></returns>
    public abstract int registerAccounts();

    /// <summary>
    /// Add buddy to buddy list and start subscribe presence
    /// </summary>
    /// <param name="ident">buddy identification</param>
    /// <returns></returns>
    public abstract int addBuddy(string ident);

    /// <summary>
    /// Delete buddy with given identification
    /// </summary>
    /// <param name="buddyId">buddy identification</param>
    /// <returns></returns>
    public abstract int delBuddy(int buddyId);

    /// <summary>
    /// Send Instant Message
    /// </summary>
    /// <param name="dest">Destination part of URI</param>
    /// <param name="message">Message Content</param>
    /// <returns></returns>
    public abstract int sendMessage(string dest, string message);

    /// <summary>
    /// Set device status for default account 
    /// </summary>
    /// <param name="accId">Account id</param>
    /// <param name="presence_state">Presence state - User Status</param>
    /// <returns></returns>
    public abstract int setStatus(int accId, EUserStatus presence_state);

    /// <summary>
    /// Set codec priority
    /// </summary>
    /// <param name="item">Codec Name</param>
    /// <param name="p">priority</param>
    public abstract void setCodecPriority(string item, int p);

    /// <summary>
    /// Get number of codecs in list
    /// </summary>
    /// <returns>Number of codecs</returns>
    public abstract int getNoOfCodecs();

    /// <summary>
    /// Get codec by index
    /// </summary>
    /// <param name="i">codec index</param>
    /// <returns>Codec Name</returns>
    public abstract string getCodec(int i);

    /// <summary>
    /// Creates an instance of call proxy 
    /// </summary>
    /// <returns></returns>
    public abstract ICallProxyInterface createCallProxy();

    #endregion
  }

  /// <summary>
  /// Call oriented interface. Offers basic session control API.
  /// </summary>
  public interface ICallProxyInterface
  {
    #region Properties
    /// <summary>
    /// Call/Session identification. All public methods refers to this identification
    /// </summary>
    int SessionId
    { get; set; }
    #endregion

    #region Public Methods
    /// <summary>
    /// Make call request
    /// </summary>
    /// <param name="dialedNo">Calling Number</param>
    /// <param name="accountId">Account Id</param>
    /// <returns>Session Identification</returns>
    int makeCall(string dialedNo, int accountId);

    /// <summary>
    /// End call
    /// </summary>
    /// <returns></returns>
    bool endCall();

    /// <summary>
    /// Report that device is alerted
    /// </summary>
    /// <returns></returns>
    bool alerted();

    /// <summary>
    /// Report that call is accepted/answered
    /// </summary>
    /// <returns></returns>
    bool acceptCall();

    /// <summary>
    /// Request call hold
    /// </summary>
    /// <returns></returns>
    bool holdCall();

    /// <summary>
    /// Request retrieve call
    /// </summary>
    /// <returns></returns>
    bool retrieveCall();

    /// <summary>
    /// Tranfer call to a given number
    /// </summary>
    /// <param name="number">Number to transfer call to</param>
    /// <returns></returns>
    bool xferCall(string number);

    /// <summary>
    /// Transfer call to partner session
    /// </summary>
    /// <param name="partnersession">Session to transfer call to</param>
    /// <returns></returns>
    bool xferCallSession(int partnersession);

    /// <summary>
    /// Request three party conference
    /// </summary>
    /// <param name="partnersession">Partner session for conference with</param>
    /// <returns></returns>
    bool threePtyCall(int partnersession);

    /// <summary>
    /// Request service (TODO)
    /// </summary>
    /// <param name="code"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    bool serviceRequest(int code, string dest);

    /// <summary>
    /// Dial digit by DTMF
    /// </summary>
    /// <param name="digits">digit string</param>
    /// <param name="mode">digit mode (TODO)</param>
    /// <returns></returns>
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

    public int makeCallByUri(string uri)
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

    public override void setCodecPriority(string item, int p)
    {
    }
    public override int getNoOfCodecs() { return 0; }

    public override string getCodec(int i) { return ""; }

    public override bool IsInitialized
    {
      get
      {
        return false;
      }
      set
      {
        ;
      }
    }

    public override ICallProxyInterface createCallProxy()
    {
      return new NullCallProxy();
    }
    #endregion

  }
  #endregion  Null Pattern

}
