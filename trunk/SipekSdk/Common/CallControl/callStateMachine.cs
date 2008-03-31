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

using System.Timers;
using System;
using Sipek.Common;

namespace Sipek.Common.CallControl
{
  /// <summary>
  /// Call control timer types
  /// </summary>
  public enum ETimerType
  {
    ENOREPLY,
    ERELEASED,
  }

  /// <summary>
  /// CStateMachine class is a telephony data container for signle call. It maintains call state, 
  /// communicates with signaling via proxy and informs about events from signaling.
  /// A Finite State Machine is implemented in State design pattern!
  /// </summary>
  public class CStateMachine
  {
    #region Variables

    private CAbstractState _state;

    // State instances....
    private CIdleState _stateIdle;
    private CConnectingState _stateCalling;
    private CAlertingState _stateAlerting;
    private CActiveState _stateActive;
    private CReleasedState _stateReleased;
    private CIncomingState _stateIncoming;
    private CHoldingState _stateHolding;

    private ECallType _callType ;
    private System.TimeSpan _duration;
    private System.DateTime _timestamp;
    private CCallManager _manager;
    protected ITimer _noreplyTimer;
    protected ITimer _releasedTimer;
    delegate bool NoReplyDelegate(int sessionId);

    #endregion Variables

    #region Properties
    /// <summary>
    /// A reference to CCallManager instance
    /// </summary>
    public CCallManager Manager
    {
      get { return _manager; }
    }

    private int _session = -1;
    /// <summary>
    /// Call/Session identification
    /// </summary>
    public int Session
    {
      get { return _session; }
      set 
      { 
        _session = value;
        // don't forget to set proxy sessionId in case of incoming call!
        this.SigProxy.SessionId = value;
      }
    }

    ////////////////////////////////////////////////////////////////////////////////
  
    private ICallProxyInterface _sigProxy;
    /// <summary>
    /// Signaling proxy instance (seperately created for each call)
    /// </summary>
    public ICallProxyInterface SigProxy
    {
      get { return _sigProxy; } 
    }

    /// <summary>
    /// Media proxy instance getter for handling tones
    /// </summary>
    public IMediaProxyInterface MediaProxy
    {
      get { return _manager.Factory.MediaProxy; }
    }

    private string _callingNumber = "";
    /// <summary>
    /// Calling number property
    /// </summary>
    public string CallingNo
    {
      get { return _callingNumber; }
      set { _callingNumber = value; }
    }

    private string _callingName = "";
    /// <summary>
    /// Calling name property
    /// </summary>
    public string CallingName
    {
      get { return _callingName; }
      set { _callingName = value; }
    }

    private bool _incoming = false;
    /// <summary>
    /// Incoming call flag
    /// </summary>
    public bool Incoming
    {
      get { return _incoming; }
      set { _incoming = value; }
    }
    /// <summary>
    /// Call type property for Call log
    /// </summary>
    public ECallType Type
    {
      get { return _callType; }
      set { _callType = value; }
    }

    /// <summary>
    /// Timestamp of a call
    /// </summary>
    public System.DateTime Time
    {
      set { _timestamp = value; }
      get { return _timestamp; }
    }

    /// <summary>
    /// Duration of a call
    /// </summary>
    public System.TimeSpan Duration
    {
      set { _duration = value; }
      get { return _duration; }
    }

    /// <summary>
    /// ???
    /// </summary>
    public System.TimeSpan RuntimeDuration
    {
      get {
        if (true == Counting)
        {
          return System.DateTime.Now.Subtract(Time);
        }
        return System.TimeSpan.Zero; 
      }
    }
    
    private bool _isHeld = false;
    /// <summary>
    /// Is call held by other side
    /// </summary>
    public bool IsHeld
    {
      get { return _isHeld; }
      set { _isHeld = value; }
    }

    private bool _is3Pty = false;
    /// <summary>
    /// Is this call 3pty (==2 active sessions)
    /// </summary>
    public bool Is3Pty
    {
      get { return _is3Pty; }
      set { _is3Pty = value; }
    }

    private bool _counting = false; // if duration counter is started
    /// <summary>
    /// ???
    /// </summary>
    public bool Counting
    {
      get { return _counting; }
      set { _counting = value; }
    }

    private bool _holdRequested = false;
    /// <summary>
    /// Has been call hold requested
    /// </summary>
    public bool HoldRequested
    {
      get { return _holdRequested; }
      set { _holdRequested = value;  }
    }

    private bool _retrieveRequested = false;
    /// <summary>
    /// Has been call retrieve requested
    /// </summary>
    public bool RetrieveRequested
    {
      get { return _retrieveRequested; }
      set { _retrieveRequested = value; }
    }
    
    /// <summary>
    /// Data access instance
    /// </summary>
    public IConfiguratorInterface Config
    {
      get { return _manager.Factory.Configurator;  }
    }

    /// <summary>
    /// Call log instance
    /// </summary>
    protected ICallLogInterface CallLoger
    {
      get { return _manager.Factory.CallLogger;  }
    }

    #endregion

    #region Constructor
    /// <summary>
    /// Call/Session constructor. Initializes call states, creates signaling proxy, initialize time,
    /// initialize timers.
    /// </summary>
    /// <param name="manager">reference to call manager</param>
    public CStateMachine(CCallManager manager)
    {
      // store manager reference...
      _manager = manager;

      // create call proxy
      _sigProxy = _manager.Factory.CommonProxy.createCallProxy();

      // initialize call states
      _stateIdle = new CIdleState(this);
      _stateAlerting = new CAlertingState(this);
      _stateActive = new CActiveState(this);
      _stateCalling = new CConnectingState(this);
      _stateReleased = new CReleasedState(this);
      _stateIncoming = new CIncomingState(this);
      _stateHolding = new CHoldingState(this);
      // change state
      _state = _stateIdle;
      
      // initialize data
      Time = System.DateTime.Now;
      Duration = System.TimeSpan.Zero;

      // Initialize timers
      if (null != _manager)
      { 
        _noreplyTimer = _manager.Factory.createTimer();
        _noreplyTimer.Interval = 15000; // hardcoded to 15s
        _noreplyTimer.Elapsed = new TimerExpiredCallback(_noreplyTimer_Elapsed);

        _releasedTimer = _manager.Factory.createTimer();
        _releasedTimer.Interval = 5000; // hardcoded to 15s
        _releasedTimer.Elapsed = new TimerExpiredCallback(_releasedTimer_Elapsed);
      }
    }

    #endregion Constructor



    void _noreplyTimer_Elapsed(object sender, EventArgs e)
    {
      this.getState().noReplyTimerExpired(this.Session);
    }

    void _releasedTimer_Elapsed(object sender, EventArgs e)
    {
      this.getState().releasedTimerExpired(this.Session);
    }

    #region Public Methods

    /// <summary>
    /// Call state getter
    /// </summary>
    /// <returns>State instance</returns>
    public CAbstractState getState()
    {
      return _state;
    }

    /// <summary>
    /// State identification getter
    /// </summary>
    /// <returns>State id</returns>
    public EStateId getStateId()
    {
      return _state.StateId;
    }

    /// <summary>
    /// State name getter
    /// </summary>
    /// <returns>State name</returns>
    public string getStateName()
    {
      return _state.Name;
    }

    /// <summary>
    /// Change state
    /// </summary>
    /// <param name="state">instance of state to change to</param>
    public void changeState(CAbstractState state)
    {
      _state.onExit();
      _state = state;
      _state.onEntry();
    }

    /// <summary>
    /// Change state by state id
    /// </summary>
    /// <param name="stateId">state id</param>
    public void changeState(EStateId stateId)
    {
      switch (stateId) 
      {
        case EStateId.IDLE:  changeState(_stateIdle); break;
        case EStateId.CONNECTING: changeState(_stateCalling); break;
        case EStateId.ALERTING: changeState(_stateAlerting); break;
        case EStateId.ACTIVE: changeState(_stateActive); break;
        case EStateId.RELEASED: changeState(_stateReleased); break;
        case EStateId.INCOMING: changeState(_stateIncoming); break;
        case EStateId.HOLDING: changeState(_stateHolding); break;
      }
      if (null != _manager) _manager.updateGui(this.Session);
    }

    /// <summary>
    /// Destroy call. Calculate call duraton time, edit call log, destroy session.
    /// </summary>
    public void destroy()
    {
      // stop tones
      MediaProxy.stopTone();
      // Calculate timing
      if (true == Counting)
      {
        Duration = System.DateTime.Now.Subtract(Time);
      }

      // update call log
      if (((Type != ECallType.EDialed) || (CallingNo.Length > 0)) && (Type != ECallType.EUndefined))
      {
        CallLoger.addCall(Type, CallingNo, CallingName, Time, Duration);
        CallLoger.save();
      } 
      // reset data
      CallingNo = "";
      Incoming = false;
      changeState(EStateId.IDLE);
      if (null != _manager) _manager.destroySession(Session);
    }

    ///////////////////////////////////////////////////////////////////////////////////
    // Timers
    /// <summary>
    /// Start timer by timer type
    /// </summary>
    /// <param name="ttype">timer type</param>
    public void startTimer(ETimerType ttype)
    {
      switch (ttype)
      {
        case ETimerType.ENOREPLY:
          _noreplyTimer.Start();
          break;
        case ETimerType.ERELEASED:
          _releasedTimer.Start();
          break;
      }
    }

    /// <summary>
    /// Stop timer by timer type
    /// </summary>
    /// <param name="ttype">timer type</param>
    public void stopTimer(ETimerType ttype)
    {
      switch (ttype)
      {
        case ETimerType.ENOREPLY:
          _noreplyTimer.Stop();
          break;
        case ETimerType.ERELEASED:
          _releasedTimer.Stop();
          break;
      }
    }

    /// <summary>
    /// Stop all timer...
    /// </summary>
    public void stopAllTimers()
    {
      _noreplyTimer.Stop();
      _releasedTimer.Stop();
      // ...
    }


    #endregion Methods
  }

} // namespace Common.CallControl
