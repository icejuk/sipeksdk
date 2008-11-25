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

using System;
using System.Collections.Generic;
using System.Text;
using Sipek.Common;
using Sipek.Common.CallControl;

#if DEBUG

using NUnit.Framework;


namespace UnitTest
{
  public class MockSipCallProxy : ICallProxyInterface
  {
    private int _sessionId = -1;
    public override int makeCall(string dialedNo, int accountId) { return 1; }

    public override bool endCall() { return true; }

    public override bool alerted() { return true; }

    public override bool acceptCall() { return true; }

    public override bool holdCall() { return true; }

    public override bool retrieveCall() { return true; }

    public override bool xferCall(string number) { return true; }

    public override bool xferCallSession(int partnersession) { return true; }

    public override bool threePtyCall(int partnersession) { return true; }

    public override bool serviceRequest(int code, string dest) { return true; }

    public override bool dialDtmf(string digits, EDtmfMode mode) { return true; }

    public override int SessionId
    {
      get
      {
        return 1;
      }
      set
      {
        _sessionId = value;
      }
    }

    public int makeCallByUri(string uri)
    {
      throw new Exception("The method or operation is not implemented.");
    }


    public static void onIncomingCall(int sessionId, string number, string info)
    {
      BaseIncomingCall(sessionId, number, info);
    }

    public static void OnCallStateChanged(int callId, ESessionState callState, string info)
    {
      BaseCallStateChanged(callId, callState, info);
    }


    public override string getCurrentCodec()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override bool conferenceCall()
    {
      throw new Exception("The method or operation is not implemented.");
    }
  }

  public class MockCommonProxy : IVoipProxy
  {
    #region CCommonProxyInterface Members
    public override int initialize()
    {
      return 0;
    }
    public override int shutdown()
    {
      return base.shutdown();
    }

    public override void setCodecPriority(string item, int p)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override int getNoOfCodecs()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override string getCodec(int i)
    {
      throw new Exception("The method or operation is not implemented.");
    }
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

    public override ICallProxyInterface createCallProxy()
    {
      return new MockSipCallProxy();
    }
    #endregion
  }

  public class MockMediaProxy : IMediaProxyInterface
  {
    public int playTone(ETones toneId)
    {
      return 1;
    }

    public int stopTone()
    {
      return 1;
    }
  }

  public class MockFactory : AbstractFactory
  {
    #region AbstractFactory Members

    public ITimer createTimer()
    {
      return new NullTimer();
    }

    public IStateMachine createStateMachine()
    {
      return new CStateMachine();
    }

    #endregion
  }

  [TestFixture]
  public class TestTelephony
  {
    MockFactory _mockFactory = new MockFactory();
    public MockCommonProxy _proxy = new MockCommonProxy();
    CCallManager _manager = CCallManager.Instance;

    [SetUp]
    public void Init()
    {
      _manager.Factory = _mockFactory;
      _manager.StackProxy = _proxy;
      _manager.Initialize();
    }

    [TearDown]
    public void Destroy()
    {
      Assert.AreEqual(0, _manager.Count);
      _manager.Shutdown();
    }

    /// <summary>
    /// Helper methods
    /// 
    /// </summary>
    /// 
    private IStateMachine makeOutgoingCall()
    {
      IStateMachine sm1 = _manager.createOutboundCall("1234");

      Assert.AreEqual(EStateId.CONNECTING, sm1.StateId);
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.State.onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm1.StateId);
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.State.onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId);
      Assert.AreEqual("ACTIVE", sm1.StateId.ToString());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(true, sm1.Counting);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      return sm1;
    }

    IStateMachine makeIncomingCall(int sessionId)
    {
      string number = "1234";
      MockSipCallProxy.OnCallStateChanged(sessionId, ESessionState.SESSION_STATE_INCOMING, "");
      MockSipCallProxy.onIncomingCall(sessionId, number, "");
      IStateMachine sm1 = _manager.getCall(sessionId);
      //sm1.State.incomingCall(number,"");

      //sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.StateId);
      Assert.AreEqual(true, sm1.Incoming);

      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);
      
      return sm1;
    }

    IStateMachine makeIncomingCallWithAnswer(int sessionId)
    {
      string number = "1234";
      //CStateMachine sm1 = new CStateMachine(null);
      //CStateMachine sm1 = _manager.createSession(sessionId, number);
      MockSipCallProxy.OnCallStateChanged(sessionId, ESessionState.SESSION_STATE_INCOMING, "");
      MockSipCallProxy.onIncomingCall(sessionId, number, "");
      IStateMachine sm1 = _manager.getCall(sessionId);
      //sm1.State.incomingCall(number, "");

      //sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.StateId);
      Assert.AreEqual(true, sm1.Incoming);

      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      _manager.onUserAnswer(sm1.Session);
      //sm1.State.acceptCall(sm1.Session);
      //sm1.changeState(EStateId.ACTIVE);
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId);
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      return sm1;
    }

    [Test]
    public void testStateMachineCreate()
    {
      CStateMachine sm = new CStateMachine();

      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero ,sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.StateId);

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.StateId);
      Assert.AreEqual("INCOMING", sm.StateId.ToString());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.StateId);
      Assert.AreEqual("ALERTING", sm.StateId.ToString());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.StateId);
      Assert.AreEqual("CONNECTING", sm.StateId.ToString());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.StateId);
      Assert.AreEqual("RELEASED", sm.StateId.ToString());

      sm.destroy();
  
    }

    [Test]
    public void testStateMachineCreateSequence()
    {
      CStateMachine sm = new CStateMachine();

      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero, sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.StateId);

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.StateId);
      Assert.AreEqual("INCOMING", sm.StateId.ToString());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.StateId);
      Assert.AreEqual("ALERTING", sm.StateId.ToString());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.StateId);
      Assert.AreEqual("CONNECTING", sm.StateId.ToString());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.StateId);
      Assert.AreEqual("RELEASED", sm.StateId.ToString());

      sm.destroy();

      // Second
      sm = new CStateMachine();
      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero, sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.StateId);

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.StateId);
      Assert.AreEqual("INCOMING", sm.StateId.ToString());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.StateId);
      Assert.AreEqual("ALERTING", sm.StateId.ToString());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.StateId);
      Assert.AreEqual("CONNECTING", sm.StateId.ToString());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.StateId);
      Assert.AreEqual("RELEASED", sm.StateId.ToString());
      sm.destroy();

      // third

      sm = new CStateMachine();
      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero, sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.StateId);

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.StateId);
      Assert.AreEqual("INCOMING", sm.StateId.ToString());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.StateId);
      Assert.AreEqual("ALERTING", sm.StateId.ToString());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.StateId);
      Assert.AreEqual("CONNECTING", sm.StateId.ToString());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.StateId);
      Assert.AreEqual("RELEASED", sm.StateId.ToString());
      sm.destroy();
    }

    [Test]
    public void testMultipleStateMachines()
    {
      CStateMachine sm1 = new CStateMachine();
      CStateMachine sm2 = new CStateMachine();
      CStateMachine sm3 = new CStateMachine();

      Assert.AreEqual(-1, sm1.Session);
      Assert.AreEqual(TimeSpan.Zero, sm1.Duration);
      Assert.AreEqual(EStateId.IDLE, sm1.StateId);

      Assert.AreEqual(-1, sm2.Session);
      Assert.AreEqual(TimeSpan.Zero, sm2.Duration);
      Assert.AreEqual(EStateId.IDLE, sm2.StateId);

      Assert.AreEqual(-1, sm3.Session);
      Assert.AreEqual(TimeSpan.Zero, sm3.Duration);
      Assert.AreEqual(EStateId.IDLE, sm3.StateId);

      // changing state
      sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.StateId);
      sm2.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm2.StateId);
      sm3.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm3.StateId);

      sm1.destroy();
      sm2.destroy();
      sm3.destroy();

      Assert.AreEqual(EStateId.IDLE, sm1.StateId);
      Assert.AreEqual(EStateId.IDLE, sm2.StateId);
      Assert.AreEqual(EStateId.IDLE, sm3.StateId);
    }

    [Test]
    public void testMultipleStateMachinesSequence()
    {
      CStateMachine sm1 = new CStateMachine();

      Assert.AreEqual(-1, sm1.Session);
      Assert.AreEqual(TimeSpan.Zero, sm1.Duration);
      Assert.AreEqual(EStateId.IDLE, sm1.StateId);

      // changing state
      sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.StateId);
      sm1.destroy();

      CStateMachine sm2 = new CStateMachine();
      Assert.AreEqual(-1, sm2.Session);
      Assert.AreEqual(TimeSpan.Zero, sm2.Duration);
      Assert.AreEqual(EStateId.IDLE, sm2.StateId);
      
      sm2.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm2.StateId);

      sm2.destroy();

      CStateMachine sm3 = new CStateMachine();
      Assert.AreEqual(-1, sm3.Session);
      Assert.AreEqual(TimeSpan.Zero, sm3.Duration);
      Assert.AreEqual(EStateId.IDLE, sm3.StateId);

      sm3.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm3.StateId);

      sm3.destroy();

      Assert.AreEqual(EStateId.IDLE, sm1.StateId);
      Assert.AreEqual(EStateId.IDLE, sm2.StateId);
      Assert.AreEqual(EStateId.IDLE, sm3.StateId);


    }

    [Test]
    public void testIncomingCall()
    {
      CStateMachine sm1 = new CStateMachine();
      Assert.AreEqual(EStateId.IDLE, sm1.StateId);
      Assert.AreEqual(false, sm1.Incoming);
      sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.StateId);
      Assert.AreEqual(true, sm1.Incoming);

      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.changeState(EStateId.ACTIVE);
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId);
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.destroy();
    }

    [Test]
    public void testOutgoingCall()
    {
      CStateMachine sm1 = new CStateMachine();
      Assert.AreEqual(EStateId.IDLE, sm1.StateId);
      Assert.AreEqual(false, sm1.Incoming);
      sm1.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm1.StateId);
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm1.StateId);
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.changeState(EStateId.ACTIVE);
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId);
      Assert.AreEqual("ACTIVE", sm1.StateId.ToString());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(true, sm1.Counting);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.destroy();
    }

    [Test]
    public void testStateMachineEventHandlingOutgoing()
    {
      CStateMachine sm1 = new CStateMachine();
      sm1.State.makeCall("1234", 0);
      Assert.AreEqual(EStateId.CONNECTING, sm1.StateId);
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNumber);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.State.onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm1.StateId);
      Assert.AreEqual(false, sm1.Counting);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.State.onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId);
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.State.onReleased();
      Assert.AreEqual(EStateId.RELEASED, sm1.StateId);
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());
    }

    [Test]
    public void testStateMachineEventHandlingIncoming()
    {
      CStateMachine sm1 = new CStateMachine();
      
      sm1.State.incomingCall("1234","");
      Assert.AreEqual(EStateId.INCOMING, sm1.StateId);
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNumber);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.State.acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId);
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.State.onReleased();
      Assert.AreEqual(EStateId.RELEASED, sm1.StateId);
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());
    }


    [Test]
    public void testCallFeaturesCallHold()
    {
      CStateMachine sm1 = new CStateMachine();

      sm1.State.incomingCall("1234","");
      Assert.AreEqual(EStateId.INCOMING, sm1.StateId);
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNumber);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.State.acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId);
      Assert.AreEqual(true, sm1.Counting);

      sm1.State.holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId); // still ACTIVE (waiting confirmation)
      sm1.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm1.StateId);
      // check twice hold
      sm1.State.holdCall();
      Assert.AreEqual(EStateId.HOLDING, sm1.StateId);

      sm1.State.retrieveCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId);

      sm1.State.holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId); // still ACTIVE (waiting confirmation)
      sm1.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm1.StateId);

      sm1.destroy();
      Assert.AreEqual(EStateId.IDLE, sm1.StateId);
    }

    [Test]
    public void testCallFeaturesCallHoldConfirm()
    {
      IStateMachine inc = this.makeIncomingCallWithAnswer(1);

      // try hold confirmation without hold request
      inc.State.onHoldConfirm();
      // no effect
      Assert.AreEqual(EStateId.ACTIVE, inc.StateId);
      Assert.AreEqual(false, inc.HoldRequested);
      // hold request
      inc.State.holdCall();
      Assert.AreEqual(true, inc.HoldRequested);
      // no effect
      Assert.AreEqual(EStateId.ACTIVE, inc.StateId);
      inc.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, inc.StateId);
      
      inc.destroy();
    }

    [Test]
    public void testCallFeaturesCallHoldMultiple()
    {
      CStateMachine sm1 = new CStateMachine();
      sm1.State.incomingCall("1234","");
      Assert.AreEqual(EStateId.INCOMING, sm1.StateId);
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNumber);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.State.acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId);
      Assert.AreEqual(true, sm1.Counting);

      sm1.State.holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId); // still ACTIVE (waiting confirmation)
      sm1.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm1.StateId);

      // next call
      CStateMachine sm2 = new CStateMachine();

      sm2.State.makeCall("4444", 0);
      Assert.AreEqual(EStateId.CONNECTING, sm2.StateId);
      Assert.AreEqual(false, sm2.Incoming);
      Assert.AreEqual("4444", sm2.CallingNumber);

      sm2.State.onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm2.StateId);
      Assert.AreEqual(false, sm2.Counting);

      sm2.State.onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm2.StateId);
      Assert.AreEqual(true, sm2.Counting);

      sm2.State.holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm2.StateId); // still ACTIVE (waiting confirmation)
      sm2.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm2.StateId);

      // release first
      sm1.State.onReleased();
      Assert.AreEqual(EStateId.RELEASED, sm1.StateId);
      sm2.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm2.StateId);

      sm2.State.endCall();
      Assert.AreEqual(EStateId.IDLE, sm2.StateId);
      sm2.State.onReleased();
      Assert.AreEqual(EStateId.IDLE, sm2.StateId);
    }

    [Test]
    public void testCallFeaturesCallWaiting()
    {
      // out call
      CStateMachine sm2 = new CStateMachine();

      sm2.State.makeCall("4444", 0);
      Assert.AreEqual(EStateId.CONNECTING, sm2.StateId);
      Assert.AreEqual(false, sm2.Incoming);
      Assert.AreEqual("4444", sm2.CallingNumber);

      sm2.State.onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm2.StateId);
      Assert.AreEqual(false, sm2.Counting);

      sm2.State.onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm2.StateId);
      Assert.AreEqual(true, sm2.Counting);

      // inc call
      CStateMachine sm1 = new CStateMachine();
      sm1.State.incomingCall("1234","");
      Assert.AreEqual(EStateId.INCOMING, sm1.StateId);
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNumber);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      // check what happens here? 
      sm1.State.acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.StateId);
      Assert.AreEqual(true, sm1.Counting);
      // this should be done automatically by call manager
      // Here we do not test call manager
      //Assert.AreEqual(EStateId.HOLDING, sm2.StateId); 

      sm1.State.endCall();
      sm2.State.endCall();
      Assert.AreEqual(EStateId.IDLE, sm1.StateId);
      Assert.AreEqual(EStateId.IDLE, sm2.StateId);
      sm1.State.onReleased();
      sm2.State.onReleased();
      Assert.AreEqual(EStateId.IDLE, sm1.StateId);
      Assert.AreEqual(EStateId.IDLE, sm2.StateId);

    }

    [Test]
    public void testCallFeaturesCallTransfer()
    {
      Assert.Ignore();
    }

    [Test]
    public void testCallFeaturesConference()
    {
      Assert.Ignore();
    }

    [Test]
    public void testCallFeaturesAutoAnswer()
    {
      Assert.Ignore();
    }

    [Test]
    public void testCallFeaturesCallForwarding()
    {
      Assert.Ignore();
    }

    /// <summary>
    /// Multicall logic. Prevents from more than 1 call becomes active!
    /// </summary>
    [Test]
    public void testCallMulticallLogicAccept2nd()
    {
      IStateMachine smOut = makeOutgoingCall();
      IStateMachine smInc = makeIncomingCall(2); // 1st call reserve sessionId 1 (nullproxy)

      // accept incoming
      _manager.onUserAnswer(smInc.Session);
      smOut.State.onHoldConfirm();

      Assert.AreEqual(EStateId.ACTIVE, smInc.StateId);
      Assert.AreEqual(EStateId.HOLDING, smOut.StateId);

      smOut.State.endCall();
      Assert.AreEqual(EStateId.IDLE, smOut.StateId);
      smInc.State.endCall();
      Assert.AreEqual(EStateId.IDLE, smInc.StateId);
    
      Assert.AreEqual(0, _manager.Count);
    }

    [Test]
    public void testCallMulticallLogicAccept2ndMore()
    {
      Assert.Ignore();
    }

    [Test]
    public void testCallMulticallLogicRetrieve2nd()
    {
      Assert.AreEqual(0, _manager.Count);

      IStateMachine smOut = makeOutgoingCall();
      IStateMachine smInc = makeIncomingCall(2); // 1st call reserve sessionId 1 (nullproxy)

      // accept incoming
      _manager.onUserAnswer(smInc.Session);
      smOut.State.onHoldConfirm();

      Assert.AreEqual(EStateId.ACTIVE, smInc.StateId);
      Assert.AreEqual(EStateId.HOLDING, smOut.StateId);

      // Retrieve 
      _manager.onUserHoldRetrieve(smOut.Session);
      smInc.State.onHoldConfirm();

      Assert.AreEqual(EStateId.HOLDING, smInc.StateId);
      Assert.AreEqual(EStateId.ACTIVE, smOut.StateId);

      smOut.State.endCall();
      Assert.AreEqual(EStateId.IDLE, smOut.StateId);
      smInc.State.endCall();
      Assert.AreEqual(EStateId.IDLE, smInc.StateId);

      Assert.AreEqual(0, _manager.Count);
    }


    [Test]
    public void testCallPendingOnUserAnswer()
    {
      IStateMachine call = this.makeOutgoingCall();
      //CStateMachine inccall = _manager.createSession(2, "1234");
      MockSipCallProxy.OnCallStateChanged(2, ESessionState.SESSION_STATE_INCOMING, "");
      MockSipCallProxy.onIncomingCall(2, "1234", "");
      IStateMachine inccall = _manager.getCall(2);
      //inccall.State.incomingCall("1234", "");
      // nothing changed yet (waiting Hold Conf)
      Assert.AreEqual(EStateId.ACTIVE, call.StateId);
      Assert.AreEqual(EStateId.INCOMING, inccall.StateId);

      _manager.onUserAnswer(inccall.Session); // set pending action
      // hold conf
      call.State.onHoldConfirm();
      // states changed
      Assert.AreEqual(EStateId.HOLDING, call.StateId);
      Assert.AreEqual(EStateId.ACTIVE, inccall.StateId);

      call.destroy();
      inccall.destroy();
    }

    [Test]
    public void testCallPendingOnUserRetrieve()
    {
      IStateMachine call = this.makeOutgoingCall();
      call.State.holdCall();
      call.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, call.StateId);

      IStateMachine inccall = this.makeIncomingCallWithAnswer(2);
      Assert.AreEqual(EStateId.ACTIVE, inccall.StateId);

      // retrieve 1st call (HOLDING)
      _manager.onUserHoldRetrieve(call.Session);
      Assert.AreEqual(EStateId.HOLDING, call.StateId);
      Assert.AreEqual(EStateId.ACTIVE, inccall.StateId);

      // hold conf
      inccall.State.onHoldConfirm();
      // states changed
      Assert.AreEqual(EStateId.ACTIVE, call.StateId);
      Assert.AreEqual(EStateId.HOLDING, inccall.StateId);

      call.destroy();
      inccall.destroy();
    }
  }

}
#endif