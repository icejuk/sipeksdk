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

/*! \mainpage Sipek Phone Project
 *
 * \section intro_sec Introduction
 *
 * SIPek is a small open source project that is intended to share common VoIP software design concepts 
 * and practices. It'd also like to become a simple and easy-to-use SIP phone with many useful features.
 * 
 * SIPek's telephony engine is based on common library used in Sipek project. The telephony part is powered 
 * by great SIP stack engine PJSIP (http://www.pjsip.org). The connection between pjsip code (C) 
 * and .Net GUI (C#) is handled by simple wrapper which is also suitable for mobile devices. Sipek use C# Audio library from http://www.codeproject.com/KB/graphics/AudioLib.aspx. 
 * The SIPek's simple software design enables efficient development, easy upgrading and 
 * user menus customizations.
 * 
 * Visit SipekSDK page at http://voipengine.googlepages.com/
 * 
 * Visit SIPek's home page at http://sipekphone.googlepages.com/ 
 * 
 *
 */


/*! \namespace CallControl
    \brief Module CallControl is a general Call Automaton engine controller. 

    Call control...
*/

using System.Collections;
using System.Collections.Generic;
using System;
using Sipek.Common;

namespace Sipek.Common.CallControl
{

  public delegate void DCallStateRefresh(int sessionId);
  public delegate void DIncomingCallNotification(int sessionId, string number, string info);  

  //////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// CCallManager
  /// Main telephony class. Manages call instances. Handles user events and dispatches to a proper 
  /// call instance automaton. 
  /// </summary>
  public class CCallManager
  {
    #region Variables

    private static CCallManager _instance = null;

    private Dictionary<int, IStateMachine> _calls;  //!< Call table

    private AbstractFactory _factory = new NullFactory();

    PendingAction _pendingAction;
 
    #endregion


    #region Properties

    public AbstractFactory Factory
    {
      get { return _factory; }
      set { _factory = value; }
    }

    private IMediaProxyInterface _media = new NullMediaProxy();
    public IMediaProxyInterface MediaProxy
    {
      get { return _media; }
      set { _media = value; }
    }

    private ICallLogInterface _callLog = new NullCallLogger();
    public ICallLogInterface CallLogger
    {
      get { return _callLog; }
      set { _callLog = value; }
    }

    private IVoipProxy _stack = new NullVoipProxy();
    public IVoipProxy StackProxy
    {
      get { return _stack; }
      set { _stack = value; }
    }

    private IConfiguratorInterface _config = new NullConfigurator();
    public IConfiguratorInterface Config
    {
      get { return _config; }
      set { _config = value; }
    }

    /// <summary>
    /// Call indexer 
    /// </summary>
    /// <param name="index">a sessionId</param>
    /// <returns>an instance of call state with provided sessionId</returns>
    public IStateMachine this[int index]
    {
      get
      {
        if (!_calls.ContainsKey(index)) return new NullStateMachine();
        return _calls[index];
      }
    }

    /// <summary>
    /// Retrieve a list of all calls (state machines)
    /// </summary>
    public Dictionary<int, IStateMachine> CallList
    {
      get { return _calls; }
    }

    public int Count
    {
      get { return _calls.Count; }
    }

    public bool Is3Pty
    {
      get 
      {
        return (getNoCallsInState(EStateId.ACTIVE) == 2) ? true : false;
      }
    }

    private bool _initialized = false;
    public bool IsInitialized
    {
      get { return _initialized; }
    }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// CCallManager Singleton
    /// </summary>
    /// <returns></returns>
    public static CCallManager Instance
    {
      get
      {
        if (_instance == null) _instance = new CCallManager();
        return _instance;
      }
    }

    #endregion Constructor

    #region Events

    /// <summary>
    /// Notify about call state changed in automaton with given sessionId
    /// </summary>
    public event DCallStateRefresh CallStateRefresh;

    public event DIncomingCallNotification IncomingCallNotification;


    /// <summary>
    /// Action definitions for pending events.
    /// </summary>
    enum EPendingActions : int
    {
      EUserAnswer,
      ECreateSession,
      EUserHold
    };

    /// <summary>
    /// Internal mechanism to execute 2 stage actions. Some user events requires 
    /// two request to VoIP side. Depending on result the second action is executed.
    /// </summary>
    class PendingAction
    {
      delegate void DPendingAnswer(int sessionId); // for onUserAnswer
      delegate void DPendingCreateSession(string number, int accountId); // for CreateOutboudCall

      private EPendingActions _actionType;
      private int _sessionId;
      private int _accountId;
      private string _number;


      public PendingAction(EPendingActions action, int sessionId)
      {
        _actionType = action;
        _sessionId = sessionId;
      }
      public PendingAction(EPendingActions action, string number, int accId)
      {
        _actionType = action;
        _sessionId = -1;
        _number = number;
        _accountId = accId;
      }

      public void Activate()
      {
        switch (_actionType)
        {
          case EPendingActions.EUserAnswer:
            CCallManager.Instance.onUserAnswer(_sessionId);
            break;
          case EPendingActions.ECreateSession:
            CCallManager.Instance.createOutboundCall(_number, _accountId);
        	  break;
          case EPendingActions.EUserHold:
            CCallManager.Instance.onUserHoldRetrieve(_sessionId);
            break;
        }
      }

    }

    /////////////////////////////////////////////////////////////////////////
    // Callback handlers
    /// <summary>
    /// Inform GUI to be refreshed 
    /// </summary>
    public void updateGui(int sessionId)
    {
      // check if call is in table (doesn't work in connecting state - session not inserted in call table)
      //if (!_calls.ContainsKey(sessionId)) return;

      if (null != CallStateRefresh) CallStateRefresh(sessionId);
    }

    #endregion Events

    #region Public methods

    public int Initialize()
    {
      return this.Initialize(_stack);
    }

    /// <summary>
    /// Initialize telephony and VoIP stack. On success register accounts.
    /// </summary>
    /// <returns>initialiation status</returns>
    public int Initialize(IVoipProxy proxy)
    {
      _stack = proxy;

      int status = 0;
      ///
      if (!IsInitialized)
      {
        //// register to signaling proxy interface
        ICallProxyInterface.CallStateChanged += OnCallStateChanged;
        ICallProxyInterface.CallIncoming += OnIncomingCall;
        ICallProxyInterface.CallNotification += OnCallNotification;

        // Initialize call table
        _calls = new Dictionary<int, IStateMachine>(); 
        
        // initialize voip proxy
        status = StackProxy.initialize();
        if (status != 0) return status;
      }

      // (re)register 
      _initialized = true;
      return status;
    }

    /// <summary>
    /// Shutdown telephony and VoIP stack
    /// </summary>
    public void Shutdown()
    {
      this.CallList.Clear();
      StackProxy.shutdown();
      _initialized = false;
      
      CallStateRefresh = null;
      IncomingCallNotification = null;

      ICallProxyInterface.CallStateChanged -= OnCallStateChanged;
      ICallProxyInterface.CallIncoming -= OnIncomingCall;
      ICallProxyInterface.CallNotification -= OnCallNotification;
    }


    static System.Threading.Mutex mutex = new System.Threading.Mutex();


    /// <summary>
    /// Create outgoing call using default accountId. 
    /// </summary>
    /// <param name="number">Number to call</param>
    public IStateMachine createOutboundCall(string number)
    {
      int accId = Config.DefaultAccountIndex;
      return this.createOutboundCall(number, accId);
    }

    /// <summary>
    /// Create outgoing call from a given account.
    /// </summary>
    /// <param name="number">Number to call</param>
    /// <param name="accountId">Specified account Id </param>
    public IStateMachine createOutboundCall(string number, int accountId)
    {
      if (!IsInitialized) return new NullStateMachine(); 

      // check if current call automatons allow session creation.
      if (this.getNoCallsInStates((int)(EStateId.CONNECTING | EStateId.ALERTING)) > 0)
      {
        // new call not allowed!
        return new NullStateMachine();
      }
      // if at least 1 connected try to put it on hold
      if (this.getNoCallsInState(EStateId.ACTIVE) == 0)
      {
        // create state machine
        IStateMachine call = Factory.createStateMachine();
        // couldn't create new call instance (max calls?)
        if (call == null)
        {
          return null;
        }

        // make call request (stack provides new sessionId)
        int newsession = call.State.makeCall(number, accountId);
        if (newsession == -1)
        {
          return new NullStateMachine();
        }
        // update call table
        // catch argument exception (same key)!!!!
        try
        {
          call.Session = newsession;
          _calls.Add(newsession, call);
        }
        catch (ArgumentException e)
        {
          // previous call not released ()
          // first release old one
          _calls[newsession].destroy();
          // and then add new one
          _calls.Add(newsession, call);
        }

        return call;
      }
      else // we have at least one ACTIVE call
      {
        // put connected call on hold
        // TODO pending action
        _pendingAction = new PendingAction(EPendingActions.ECreateSession, number, accountId);
        IStateMachine call = getCallInState(EStateId.ACTIVE); 
        call.State.holdCall();
      }
      return new NullStateMachine();
    }

    /// <summary>
    /// Destroy call 
    /// </summary>
    /// <param name="session">session identification</param>
    public void destroySession(int session)
    {
      _calls.Remove(session);
      // Warning: this call no longer exists
      updateGui(session);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public IStateMachine getCall(int session)
    {
      if ((_calls.Count == 0) || (!_calls.ContainsKey(session))) return new NullStateMachine();
      return _calls[session];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    /// <param name="stateId"></param>
    /// <returns></returns>
    public IStateMachine getCallInState(EStateId stateId)
    {
      if (_calls.Count == 0) return new NullStateMachine();
      foreach (KeyValuePair<int, IStateMachine> call in _calls)
      {
        if (call.Value.State.Id == stateId) return call.Value;
      }
      return new NullStateMachine();
    }

    public int getNoCallsInState(EStateId stateId)
    {
      int cnt = 0;
      foreach (KeyValuePair<int, IStateMachine> kvp in _calls)
      {
        if (stateId == kvp.Value.State.Id)
        {
          cnt++;
        }
      }
      return cnt;
    }

    private int getNoCallsInStates(int states)
    {
      int cnt = 0;
      foreach (KeyValuePair<int, IStateMachine> kvp in _calls)
      {
        if ((states & (int)kvp.Value.State.Id) == (int)kvp.Value.State.Id)
        {
          cnt++;
        }
      }
      return cnt;
    }

    /// <summary>
    /// Collect state machines in a given state
    /// </summary>
    /// <param name="stateId">state machine state</param>
    /// <returns>List of state machines</returns>
    public ICollection<IStateMachine> enumCallsInState(EStateId stateId)
    {
      List<IStateMachine> list = new List<IStateMachine>();

      foreach (KeyValuePair<int, IStateMachine> kvp in _calls)
      {
        if (stateId == kvp.Value.State.Id)
        {
          list.Add(kvp.Value);
        }
      }
      return list;
    }


    /// <summary>
    /// User triggers a call release for a given session
    /// </summary>
    /// <param name="session">session identification</param>
    public void onUserRelease(int session)
    {
      this[session].State.endCall();
    }

    /// <summary>
    /// User accepts call for a given session
    /// In case of multi call put current active call to Hold
    /// </summary>
    /// <param name="session">session identification</param>
    public void onUserAnswer(int session)
    {
      List<IStateMachine> list = (List<IStateMachine>)this.enumCallsInState(EStateId.ACTIVE);
      // should not be more than 1 call active
      if (list.Count > 0)
      {
        // put it on hold
        IStateMachine sm = list[0];
        if (!sm.IsNull) sm.State.holdCall();

        // set ANSWER event pending for HoldConfirm
        // TODO
        _pendingAction = new PendingAction(EPendingActions.EUserAnswer, session);
        return;
      }
      this[session].State.acceptCall();
    }

    /// <summary>
    /// User put call on hold or retrieve 
    /// </summary>
    /// <param name="session">session identification</param>
    public void onUserHoldRetrieve(int session)
    {
      // check Hold or Retrieve
      IAbstractState state = this[session].State;
      if (state.Id == EStateId.ACTIVE)
      {
        this.getCall(session).State.holdCall();
      }
      else if (state.Id == EStateId.HOLDING)
      {
        // execute retrieve
        // check if any ACTIVE calls
        if (this.getNoCallsInState(EStateId.ACTIVE) > 0)
        {
          // get 1st and put it on hold
          IStateMachine sm = ((List<IStateMachine>)enumCallsInState(EStateId.ACTIVE))[0];
          if (!sm.IsNull) sm.State.holdCall();

          // set Retrieve event pending for HoldConfirm
          _pendingAction = new PendingAction(EPendingActions.EUserHold, session);
          return;
        }

        this[session].State.retrieveCall();
      }
      else
      {
        // illegal
      }
    }

    /// <summary>
    /// User starts a call transfer
    /// </summary>
    /// <param name="session">session identification</param>
    /// <param name="number">number to transfer</param>
    public void onUserTransfer(int session, string number)
    {
      this[session].State.xferCall(number);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    /// <param name="digits"></param>
    /// <param name="mode"></param>
    public void onUserDialDigit(int session, string digits, EDtmfMode mode)
    {
      this[session].State.dialDtmf(digits, mode);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    public void onUserConference(int session)
    {
      // check preconditions: 1 call active, other held
      // 1st if current call is held -> search if any active -> execute retrieve
      if ((getNoCallsInState(EStateId.ACTIVE) == 1)&&(getNoCallsInState(EStateId.HOLDING) >= 1))
      {
        IStateMachine call = getCallInState(EStateId.HOLDING);
        call.State.retrieveCall();
        // set conference flag
        return;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void activatePendingAction()
    {
      if (null != _pendingAction) _pendingAction.Activate();
      _pendingAction = null;
    }
    
    #endregion  // public methods

    #region Private Methods

    ////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 
    /// </summary>
    /// <param name="callId"></param>
    /// <param name="callState"></param>
    private void OnCallStateChanged(int callId, ESessionState callState, string info)
    {
      if (callState == ESessionState.SESSION_STATE_INCOMING)
      {
        IStateMachine incall = Factory.createStateMachine();
        // couldn't create new call instance (max calls?)
        if (incall.IsNull)
        {
          // check if CFB, activate redirection
          if (Config.CFBFlag == true)
          {
            // get stack proxy
            ICallProxyInterface proxy = StackProxy.createCallProxy();
            // assign callid to the proxy...
            //proxy.SessionId = callId;
            proxy.serviceRequest((int)EServiceCodes.SC_CFB, Config.CFBNumber);
            return;
          }
        }
        // save session parameters
        incall.Session = callId;
        // add call to call table
        _calls.Add(callId, incall);

        return;
      }

      IStateMachine call = getCall(callId);
      if (call.IsNull) return;

      switch (callState)
      {
        case ESessionState.SESSION_STATE_CALLING:
          //sm.getState().onCalling();
          break;
        case ESessionState.SESSION_STATE_EARLY:
          call.State.onAlerting();
          break;
        case ESessionState.SESSION_STATE_CONNECTING:
          call.State.onConnect();
          break;
        case ESessionState.SESSION_STATE_DISCONNECTED:
          call.State.onReleased();
          break;
      }
    }

    /// <summary>
    /// Create session for incoming call.
    /// </summary>
    /// <param name="sessionId">session identification</param>
    /// <param name="number">number from calling party</param>
    /// <param name="info">additional info of calling party</param>
    private void OnIncomingCall(int sessionId, string number, string info)
    {
      IStateMachine call = getCall(sessionId);

      if (call.IsNull) return;

      // inform automaton for incoming call
      call.State.incomingCall(number, info);

      // call callback 
      if (IncomingCallNotification != null) IncomingCallNotification(sessionId, number, info);
    }

    private void OnCallNotification(int callId, ECallNotification notFlag, string text)
    {
      if (notFlag == ECallNotification.CN_HOLDCONFIRM)
      {
        IStateMachine sm = this.getCall(callId);
        if (!sm.IsNull) sm.State.onHoldConfirm();
      }
    }

    #endregion Methods

  }

} // namespace Sipek
