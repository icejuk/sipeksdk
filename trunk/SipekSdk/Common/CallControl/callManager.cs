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

    private Dictionary<int, CStateMachine> _calls;  //!< Call table

    private AbstractFactory _factory = new NullFactory();

    PendingAction _pendingAction;
 
    #endregion


    #region Properties

    public AbstractFactory Factory
    {
      get { return _factory; }
      set { _factory = value; }
    }

    public IConfiguratorInterface Config
    {
      get { return Factory.getConfigurator(); }
    }

    /// <summary>
    /// Call indexer 
    /// </summary>
    /// <param name="index">a sessionId</param>
    /// <returns>an instance of call state with provided sessionId</returns>
    public CStateMachine this[int index]
    {
      get
      {
        if (!_calls.ContainsKey(index)) return null;
        return _calls[index];
      }
    }

    /// <summary>
    /// Retrieve a list of all calls (state machines)
    /// </summary>
    public Dictionary<int, CStateMachine> CallList
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
    public bool isInitialized
    {
      get { return _initialized; }
    }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// CCallManager Singleton
    /// </summary>
    /// <returns></returns>
    public static CCallManager getInstance()
    { 
      if (_instance == null)
      {
        _instance = new CCallManager();
      }
      return _instance;
    }

    #endregion Constructor

    #region Events

    public delegate void DCallStateRefresh(int sessionId);  // define callback type 
    /// <summary>
    /// Notify about call state changed in automaton with given sessionId
    /// </summary>
    public event DCallStateRefresh CallStateRefresh;

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
            CCallManager.getInstance().onUserAnswer(_sessionId);
            break;
          case EPendingActions.ECreateSession:
            CCallManager.getInstance().createOutboundCall(_number, _accountId);
        	  break;
          case EPendingActions.EUserHold:
            CCallManager.getInstance().onUserHoldRetrieve(_sessionId);
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
      if (null != CallStateRefresh) CallStateRefresh(sessionId);
    }

    #endregion Events

    #region Public methods

    /// <summary>
    /// Initialize telephony and VoIP stack. On success register accounts.
    /// </summary>
    /// <returns>initialiation status</returns>
    public int Initialize()
    {
      int status = 0;
      ///
      if (!isInitialized)
      {
        // register to signaling proxy interface
        Factory.getCommonProxy().CallStateChanged += OnCallStateChanged;
        Factory.getCommonProxy().CallIncoming += OnIncomingCall;
        Factory.getCommonProxy().CallNotification += OnCallNotification;

        // Initialize call table
        _calls = new Dictionary<int, CStateMachine>(); 
        
        // initialize voip proxy
        status = Factory.getCommonProxy().initialize();
        if (status != 0) return status;
      }

      // (re)register 
      Factory.getCommonProxy().registerAccounts(); 

      _initialized = true;
      return status;
    }

    /// <summary>
    /// Shutdown telephony and VoIP stack
    /// </summary>
    public void Shutdown()
    {
      this.CallList.Clear();
      Factory.getCommonProxy().shutdown();
      _initialized = false;
      this.CallStateRefresh = null;
    }


    /// <summary>
    /// Create outgoing call using default accountId. 
    /// </summary>
    /// <param name="number">Number to call</param>
    public CStateMachine createOutboundCall(string number)
    {
      int accId = Config.DefaultAccountIndex;
      return this.createOutboundCall(number, accId);
    }

    /// <summary>
    /// Create outgoing call from a given account.
    /// </summary>
    /// <param name="number">Number to call</param>
    /// <param name="accountId">Specified account Id </param>
    public CStateMachine createOutboundCall(string number, int accountId)
    {
      // check if current call automatons allow session creation.
      if (this.getNoCallsInStates((int)(EStateId.CONNECTING | EStateId.ALERTING)) > 0)
      {
        // new call not allowed!
        return null;
      }
      // if at least 1 connected try to put it on hold
      if (this.getNoCallsInState(EStateId.ACTIVE) == 0)
      {
        // create state machine
        // TODO check max calls!!!!
        CStateMachine call = new CStateMachine(this);

        // make call request (stack provides new sessionId)
        int newsession = call.getState().makeCall(number, accountId);
        if (newsession == -1)
        {
          return null;
        }
        // update call table
        // TODO catch argument exception (same key)!!!!
        call.Session = newsession;
        _calls.Add(newsession, call);
        return call;
      }
      else // we have at least one ACTIVE call
      {
        // put connected call on hold
        // TODO pending action
        _pendingAction = new PendingAction(EPendingActions.ECreateSession, number, accountId);
        CStateMachine call = getCallInState(EStateId.ACTIVE); 
        call.getState().holdCall();
      }
      return null;
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
    public CStateMachine getCall(int session)
    {
      if ((_calls.Count == 0) || (!_calls.ContainsKey(session))) return null;
      return _calls[session];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    /// <param name="stateId"></param>
    /// <returns></returns>
    public CStateMachine getCallInState(EStateId stateId)
    {
      if (_calls.Count == 0)  return null;
      foreach (KeyValuePair<int, CStateMachine> call in _calls)
      {
        if (call.Value.getStateId() == stateId) return call.Value;
      }
      return null;
    }

    public int getNoCallsInState(EStateId stateId)
    {
      int cnt = 0;
      foreach (KeyValuePair<int, CStateMachine> kvp in _calls)
      {
        if (stateId == kvp.Value.getStateId())
        {
          cnt++;
        }
      }
      return cnt;
    }

    private int getNoCallsInStates(int states)
    {
      int cnt = 0;
      foreach (KeyValuePair<int, CStateMachine> kvp in _calls)
      {
        if ((states & (int)kvp.Value.getStateId()) == (int)kvp.Value.getStateId())
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
    public ICollection<CStateMachine> enumCallsInState(EStateId stateId)
    {
      List<CStateMachine> list = new List<CStateMachine>();

      foreach (KeyValuePair<int, CStateMachine> kvp in _calls)
      {
        if (stateId == kvp.Value.getStateId())
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
      this[session].getState().endCall();
    }

    /// <summary>
    /// User accepts call for a given session
    /// In case of multi call put current active call to Hold
    /// </summary>
    /// <param name="session">session identification</param>
    public void onUserAnswer(int session)
    {
      List<CStateMachine> list = (List<CStateMachine>)this.enumCallsInState(EStateId.ACTIVE);
      // should not be more than 1 call active
      if (list.Count > 0)
      {
        // put it on hold
        CStateMachine sm = list[0];
        if (null != sm) sm.getState().holdCall();

        // set ANSWER event pending for HoldConfirm
        // TODO
        _pendingAction = new PendingAction(EPendingActions.EUserAnswer, session);
        return;
      }
      this[session].getState().acceptCall();
    }

    /// <summary>
    /// User put call on hold or retrieve 
    /// </summary>
    /// <param name="session">session identification</param>
    public void onUserHoldRetrieve(int session)
    {
      // check Hold or Retrieve
      CAbstractState state = this[session].getState();
      if (state.StateId == EStateId.ACTIVE)
      {
        this.getCall(session).getState().holdCall();
      }
      else if (state.StateId == EStateId.HOLDING)
      {
        // execute retrieve
        // check if any ACTIVE calls
        if (this.getNoCallsInState(EStateId.ACTIVE) > 0)
        {
          // get 1st and put it on hold
          CStateMachine sm = ((List<CStateMachine>)enumCallsInState(EStateId.ACTIVE))[0];
          if (null != sm) sm.getState().holdCall();

          // set Retrieve event pending for HoldConfirm
          _pendingAction = new PendingAction(EPendingActions.EUserHold, session);
          return;
        }

        this[session].getState().retrieveCall();
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
      this[session].getState().xferCall(number);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    /// <param name="digits"></param>
    /// <param name="mode"></param>
    public void onUserDialDigit(int session, string digits, int mode)
    {
      this[session].getState().dialDtmf(digits, 0);
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
        CStateMachine call = getCallInState(EStateId.HOLDING);
        call.getState().retrieveCall();
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
    private void OnCallStateChanged(int callId, int callState, string info)
    {
      //    PJSIP_INV_STATE_NULL,	    /**< Before INVITE is sent or received  */
      //    PJSIP_INV_STATE_CALLING,	    /**< After INVITE is sent		    */
      //    PJSIP_INV_STATE_INCOMING,	    /**< After INVITE is received.	    */
      //    PJSIP_INV_STATE_EARLY,	    /**< After response with To tag.	    */
      //    PJSIP_INV_STATE_CONNECTING,	    /**< After 2xx is sent/received.	    */
      //    PJSIP_INV_STATE_CONFIRMED,	    /**< After ACK is sent/received.	    */
      //    PJSIP_INV_STATE_DISCONNECTED,   /**< Session is terminated.		    */
      //if (callState == 2) return 0;

      CStateMachine sm = getCall(callId);
      if (sm == null) return;

      switch (callState)
      {
        case 1:
          //sm.getState().onCalling();
          break;
        case 2:
          //sm.getState().incomingCall("4444");
          break;
        case 3:
          sm.getState().onAlerting();
          break;
        case 4:
          sm.getState().onConnect();
          break;
        case 6:
          sm.getState().onReleased();
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
      CStateMachine call = new CStateMachine(this);

      if (null == call) return;

      // save session parameters
      call.Session = sessionId;
      // add call to call table
      _calls.Add(sessionId, call);

      // inform automaton for incoming call
      call.getState().incomingCall(number, info);
    }

    private void OnCallNotification(int callId, ECallNotification notFlag, string text)
    {
      if (notFlag == ECallNotification.CN_HOLDCONFIRM)
      {
        CStateMachine sm = this.getCall(callId);
        if (sm != null) sm.getState().onHoldConfirm();
      }
    }

    #endregion Methods

  }

} // namespace Sipek
