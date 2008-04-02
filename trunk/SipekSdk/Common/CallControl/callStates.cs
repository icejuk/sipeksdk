/* 
 * Copyright (C) 2007 Sasa Coh <sasacoh@gmail.com>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 
 */

using Sipek.Common;


namespace Sipek.Common.CallControl
{
  #region Enums
  /// <summary>
  /// Call state Ids
  /// </summary>
  public enum EStateId  : int 
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

  #region AbstractState
  /// <summary>
  /// CAbstractState implements ICallProxyInterface interface. 
  /// The interface is used for sending requests to call server.
  /// It's a base for all call states used by CStateMachine. 
  /// </summary>
  public abstract class CAbstractState : ICallProxyInterface
  {

    #region Properties
    private EStateId _stateId = EStateId.IDLE;
    /// <summary>
    /// State identification property
    /// </summary>
    public EStateId StateId 
    {
      get { return _stateId;  }
      set { _stateId = value; }
    }
    /// <summary>
    /// State name property
    /// </summary>
    public string Name
    {
      get {
        return StateId.ToString(); 
      }
    }
    /// <summary>
    /// Signaling proxy instance for communication with VoIP stack
    /// </summary>
    public ICallProxyInterface CallProxy
    {
      get { return _smref.SigProxy; }
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

    #endregion

    #region Variables

    protected CStateMachine _smref;

    #endregion Variables

    #region Constructor
    /// <summary>
    /// Abstract state construction.
    /// </summary>
    /// <param name="sm">reference to call state machine</param>
    public CAbstractState(CStateMachine sm)
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


  #region IdleState
  /// <summary>
  /// State Idle indicates the call is inactive
  /// </summary>
  public class CIdleState : CAbstractState
  {
    public CIdleState(CStateMachine sm) 
      : base(sm)
    {
      StateId = EStateId.IDLE;
    }

    public override void onEntry()
    {
    }

    public override void onExit()
    {
    }

    public override int makeCall(string dialedNo, int accountId)
    {
      _smref.CallingNo = dialedNo;
      _smref.changeState(EStateId.CONNECTING);
      return CallProxy.makeCall(dialedNo, accountId);
    }

    public override void incomingCall(string callingNo, string display)
    {
      _smref.CallingNo = callingNo;
      _smref.CallingName = display;
      _smref.changeState(EStateId.INCOMING);
    }

  }
  #endregion 

  #region ConnectingState
  /// <summary>
  /// Connecting states indicates outgoing call has been initiated and waiting for a response.
  /// </summary>
  public class CConnectingState : CAbstractState
  {
    public CConnectingState(CStateMachine sm) 
      : base(sm)
    {
      StateId = EStateId.CONNECTING;
    }

    public override void onEntry()
    {
      _smref.Type = ECallType.EDialed;
    }

    public override void onExit()
    {
    }

    public override void onReleased()
    {
      //_smref.destroy();
      _smref.changeState(EStateId.RELEASED);
    }

    public override void onAlerting()
    {
      _smref.changeState(EStateId.ALERTING);
    }


    public override void onConnect()
    {
      _smref.changeState(EStateId.ACTIVE);
    }

    public override bool endCall()
    {
      CallProxy.endCall();
      _smref.destroy();
      return base.endCall();
    }

  }
  #endregion

  #region AlertingState
  /// <summary>
  /// Alerting state indicates other side accepts the call. Play ring back tone.
  /// </summary>
  public class CAlertingState : CAbstractState
  {
    public CAlertingState(CStateMachine sm)
      : base(sm)
    {
      StateId = EStateId.ALERTING;
    }

    public override void onEntry()
    {
      MediaProxy.playTone(ETones.EToneRingback);
    }

    public override void onExit()
    {
      MediaProxy.stopTone();
    }

    public override void onConnect()
    {
      _smref.Time = System.DateTime.Now;
      _smref.changeState(EStateId.ACTIVE);
    }

    public override void onReleased()
    {
      _smref.changeState(EStateId.RELEASED);
    }

    public override bool endCall()
    {
      CallProxy.endCall();
      _smref.destroy();
      return base.endCall();
    }
  }
  #endregion

  #region ActiveState
  /// <summary>
  /// Active state indicates converstation. 
  /// </summary>
  public class CActiveState : CAbstractState
  {
    public CActiveState(CStateMachine sm) 
      : base(sm)
    {
      StateId = EStateId.ACTIVE;
    }

    public override void onEntry()
    {
      _smref.Counting = true;
    }

    public override void onExit()
    {
    }

    public override bool endCall()
    {
      _smref.Duration = System.DateTime.Now.Subtract(_smref.Time);

      CallProxy.endCall();
      _smref.destroy();
      return base.endCall();
    }

    public override bool holdCall()
    {
      _smref.HoldRequested = true;
      return CallProxy.holdCall();
    }

    public override bool xferCall(string number)
    {
      return CallProxy.xferCall(number);
    }
    public override bool xferCallSession(int partnersession)
    {
      return CallProxy.xferCallSession(partnersession);
    }

    public override void onHoldConfirm()
    {
      // check if Hold requested
      if (_smref.HoldRequested)
      {
        _smref.changeState(EStateId.HOLDING);
        // activate pending action if any
        _smref.Manager.activatePendingAction();
      }
      _smref.HoldRequested = false;
    }

    public override void onReleased()
    {
      _smref.changeState(EStateId.RELEASED);
    }
  }
  #endregion

  #region ReleasedState
  /// <summary>
  /// Released State indicates call has been released and waiting for destruction.
  /// </summary>
  public class CReleasedState : CAbstractState
  {
    public CReleasedState(CStateMachine sm)
      : base(sm)
    {
      StateId = EStateId.RELEASED;
    }

    public override void onEntry()
    {
      MediaProxy.playTone(ETones.EToneCongestion);
      _smref.startTimer(ETimerType.ERELEASED);
    }

    public override void onExit()
    {
      MediaProxy.stopTone();
      _smref.stopAllTimers();
    }

    public override bool endCall()
    {
      _smref.destroy();
      return true;
    }

    public override bool releasedTimerExpired(int sessionId)
    {
      _smref.destroy();
      return true;
    }
  }
  #endregion

  #region IncomingState
  /// <summary>
  /// Incoming state indicates incoming call. Check CFx and DND features. Start ringer. 
  /// </summary>
  public class CIncomingState : CAbstractState
  {
    public CIncomingState(CStateMachine sm)
      : base(sm)
    {
      StateId = EStateId.INCOMING;
    }

    public override void onEntry()
    {
      _smref.Incoming = true;

      int sessionId = SessionId;

      if ((_smref.Config.CFUFlag == true) && (_smref.Config.CFUNumber.Length > 0))
      {
        CallProxy.serviceRequest((int)EServiceCodes.SC_CFU, _smref.Config.CFUNumber);
      }
      else if (_smref.Config.DNDFlag == true)
      {
        CallProxy.serviceRequest((int)EServiceCodes.SC_DND, "");
      }
      else if (_smref.Config.AAFlag == true)
      {
        this.acceptCall();
      }
      else
      {
        CallProxy.alerted();
        _smref.Type = ECallType.EMissed;
        MediaProxy.playTone(ETones.EToneRing);
      }

      // if CFNR active start timer
      if (_smref.Config.CFNRFlag)
      {
        _smref.startTimer(ETimerType.ENOREPLY);
      }
    }

    public override void onExit()
    {
      MediaProxy.stopTone();
    }

    public override bool acceptCall()
    {
      _smref.Type = ECallType.EReceived;
      _smref.Time = System.DateTime.Now;

      CallProxy.acceptCall();
      _smref.changeState(EStateId.ACTIVE);
      return true;
    }

    public override void onReleased()
    {
      _smref.changeState(EStateId.RELEASED);
      _smref.destroy();
    }

    public override bool xferCall(string number)
    {
      // In fact this is not Tranfser. It's Deflect => redirect...
      return CallProxy.serviceRequest((int)EServiceCodes.SC_CD, number);
    }

    public override bool endCall()
    {
      CallProxy.endCall();
      _smref.destroy();
      return base.endCall();
    }

    public override bool noReplyTimerExpired(int sessionId)
    {
      CallProxy.serviceRequest((int)EServiceCodes.SC_CFNR, _smref.Config.CFUNumber);
      return true;
    }

  }
  #endregion

  #region HoldingState
  /// <summary>
  /// Holding state indicates call is hodling.
  /// </summary>
  public class CHoldingState : CAbstractState
  {
    public CHoldingState(CStateMachine sm)
      : base(sm)
    {
      StateId = EStateId.HOLDING;
    }

    public override void onEntry()
    {
    }

    public override void onExit()
    {
    }

    public override bool retrieveCall()
    {
      _smref.RetrieveRequested = true;
      CallProxy.retrieveCall();
      _smref.changeState(EStateId.ACTIVE);
      return true;
    }

    // TODO implement in stack interface
    //public override onRetrieveConfirm()
    //{
    //  if (_smref.RetrieveRequested)
    //  {
    //    _smref.changeState(EStateId.ACTIVE);
    //  }
    //  _smref.RetrieveRequested = false;
    //}

    public override void onReleased()
    {
      //_smref.destroy();
      _smref.changeState(EStateId.RELEASED);
    }

    public override bool endCall()
    {
      CallProxy.endCall();
      _smref.destroy();
      return base.endCall();
    }
  }
  #endregion

} // namespace Sipek
