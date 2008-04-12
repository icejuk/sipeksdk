using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{
  #region Enums
  /// <summary>
  /// Call state Ids
  /// </summary>
  public enum EStateId : int
  {
    IDLE = 0x1,
    CONNECTING = 0x2,
    ALERTING = 0x4,
    ACTIVE = 0x8,
    RELEASED = 0x10,
    INCOMING = 0x20,
    HOLDING = 0x40
  }

  #endregion

  #region IAbstractState
  /// <summary>
  /// CAbstractState implements ICallProxyInterface interface. 
  /// The interface is used for sending requests to call server.
  /// It's a base for all call states used by CStateMachine. 
  /// </summary>
  public abstract class IAbstractState : ICallProxyInterface
  {

    #region Properties
    private EStateId _stateId = EStateId.IDLE;
    /// <summary>
    /// State identification property
    /// </summary>
    public EStateId Id
    {
      get { return _stateId; }
      set { _stateId = value; }
    }

    /// <summary>
    /// Signaling proxy instance for communication with VoIP stack
    /// </summary>
    public ICallProxyInterface CallProxy
    {
      get { return _smref.CallProxy; }
    }
    /// <summary>
    /// Media proxy instance for handling tones
    /// </summary>
    public IMediaProxyInterface MediaProxy
    {
      get { return _smref.MediaProxy; }
    }
    /// <summary>
    /// Call/Session identification
    /// </summary>
    public int SessionId
    {
      get { return _smref.Session; }
      set { }
    }

    public override string ToString()
    {
      return _stateId.ToString();
    }

    #endregion

    #region Variables

    protected IStateMachine _smref;

    #endregion Variables

    #region Constructor
    /// <summary>
    /// Abstract state construction.
    /// </summary>
    /// <param name="sm">reference to call state machine</param>
    public IAbstractState(IStateMachine sm)
    {
      _smref = sm;
    }

    #endregion Constructor

    #region Abstract Methods

    /// <summary>
    /// State entry method
    /// </summary>
    public abstract void onEntry();
    /// <summary>
    /// State exit method
    /// </summary>
    public abstract void onExit();

    /// <summary>
    /// Reply timer 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public virtual bool noReplyTimerExpired(int sessionId) { return false; }
    /// <summary>
    /// Released timer
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public virtual bool releasedTimerExpired(int sessionId) { return false; }
    #endregion

    #region Inherited methods

    public virtual int makeCall(string dialedNo, int accountId)
    {
      return -1;
    }

    public virtual bool endCall()
    {
      return true;
    }

    public virtual bool acceptCall()
    {
      return true;
    }


    public virtual bool alerted()
    {
      return true;
    }

    public virtual bool holdCall()
    {
      return true;
    }

    public virtual bool retrieveCall()
    {
      return true;
    }
    public virtual bool xferCall(string number)
    {
      return true;
    }
    public virtual bool xferCallSession(int partnersession)
    {
      return true;
    }
    public virtual bool threePtyCall(int partnersession)
    {
      return true;
    }

    public virtual bool serviceRequest(int code, string dest)
    {
      CallProxy.serviceRequest(code, dest);
      return true;
    }

    public virtual bool dialDtmf(string digits, int mode)
    {
      CallProxy.dialDtmf(digits, mode);
      return true;
    }

    #endregion Methods

    #region Callbacks

    public virtual void incomingCall(string callingNo, string display)
    {
    }

    public virtual void onAlerting()
    {
    }

    public virtual void onConnect()
    {
    }

    public virtual void onReleased()
    {
    }

    public virtual void onHoldConfirm()
    {
    }
    #endregion Callbacks
  }
  #endregion

  internal class NullState : IAbstractState
  {
    public NullState()
      : base(new NullStateMachine())
    { }

    public override void onEntry()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override void onExit()
    {
      throw new Exception("The method or operation is not implemented.");
    }
  }
}
