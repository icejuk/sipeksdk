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
using NUnit.Framework;
using Sipek.Common;
using Sipek.Common.CallControl;


namespace UnitTest
{
  public class MockSipProxy : ICallProxyInterface
  {
    public int makeCall(string dialedNo, int accountId) { return 1; }

    public bool endCall() { return true; }

    public bool alerted() { return true; }

    public bool acceptCall() { return true; }

    public bool holdCall() { return true; }

    public bool retrieveCall() { return true; }

    public bool xferCall(string number) { return true; }

    public bool xferCallSession(int partnersession) { return true; }

    public bool threePtyCall(int partnersession) { return true; }

    public bool serviceRequest(int code, string dest) { return true; }

    public bool dialDtmf(string digits, int mode) { return true; }

    public int SessionId
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

  public class MockCommonProxy : IVoipProxy
  {
    #region CCommonProxyInterface Members
    public override int initialize()
    {
      return 1;
    }
    public override int shutdown()
    {
      return base.shutdown();
    }

    public override int registerAccounts()
    {
      return registerAccounts();
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

    public override int setStatus(int accId, EUserStatus status)
    {
      return 1;
    }

    public override void setCodecPrioroty(string item, int p)
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

    public void onIncomingCall(int sessionId, string number, string info)
    {
      BaseIncomingCall(sessionId, number, info);
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
    IVoipProxy _proxy;
    NullCallLogger _calllogger = new NullCallLogger();
    NullMediaProxy _mediaproxy = new NullMediaProxy();
    NullConfigurator _config = new NullConfigurator();
    NullCallProxy _callProxy = new NullCallProxy();

    public IVoipProxy CommonProxy
    {
      set { _proxy = value; }
    }

    #region AbstractFactory Members

    public ICallProxyInterface createCallProxy()
    {
      return _callProxy;
    }

    public ITimer createTimer()
    {
      return new NullTimer();
    }

    public ICallLogInterface getCallLogger()
    {
      return _calllogger ;
    }

    public IVoipProxy getCommonProxy()
    {
      return _proxy;
    }

    public IConfiguratorInterface getConfigurator()
    {
      return _config;
    }

    public IMediaProxyInterface getMediaProxy()
    {
      return _mediaproxy;
    }

    #endregion
  }

  [TestFixture]
  public class TestTelephony
  {
    MockFactory _mockFactory = new MockFactory();
    public MockCommonProxy _proxy = new MockCommonProxy();
    CCallManager _manager = CCallManager.getInstance();

    [SetUp]
    public void Init()
    {
      _mockFactory.CommonProxy = _proxy;
      _manager.Factory = _mockFactory;
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
    private CStateMachine makeOutgoingCall()
    {
      CStateMachine sm1 = _manager.createOutboundCall("1234");

      Assert.AreEqual(EStateId.CONNECTING, sm1.getStateId());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.getState().onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm1.getStateId());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.getState().onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual("ACTIVE", sm1.getStateName());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(true, sm1.Counting);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      return sm1;
    }

    CStateMachine makeIncomingCall(int sessionId)
    {
      string number = "1234";
      //CStateMachine sm1 = new CStateMachine(null);
      //CStateMachine sm1 = _manager.createOutboundCall(sessionId, number);
      _proxy.onIncomingCall(sessionId, number, "");
      CStateMachine sm1 = _manager.getCall(sessionId);
      //sm1.getState().incomingCall(number,"");

      //sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);

      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);
      
      return sm1;
    }

    CStateMachine makeIncomingCallWithAnswer(int sessionId)
    {
      string number = "1234";
      //CStateMachine sm1 = new CStateMachine(null);
      //CStateMachine sm1 = _manager.createSession(sessionId, number);
      _proxy.onIncomingCall(sessionId, number, "");
      CStateMachine sm1 = _manager.getCall(sessionId);
      //sm1.getState().incomingCall(number, "");

      //sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);

      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      _manager.onUserAnswer(sm1.Session);
      //sm1.getState().acceptCall(sm1.Session);
      //sm1.changeState(EStateId.ACTIVE);
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      return sm1;
    }

    [Test]
    public void testStateMachineCreate()
    {
      CStateMachine sm = new CStateMachine(_manager);

      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero ,sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.getStateId());

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.getStateId());
      Assert.AreEqual("INCOMING", sm.getStateName());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.getStateId());
      Assert.AreEqual("ALERTING", sm.getStateName());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.getStateId());
      Assert.AreEqual("CONNECTING", sm.getStateName());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.getStateId());
      Assert.AreEqual("RELEASED", sm.getStateName());

      sm.destroy();
  
    }

    [Test]
    public void testStateMachineCreateSequence()
    {
      CStateMachine sm = new CStateMachine(_manager);

      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero, sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.getStateId());

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.getStateId());
      Assert.AreEqual("INCOMING", sm.getStateName());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.getStateId());
      Assert.AreEqual("ALERTING", sm.getStateName());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.getStateId());
      Assert.AreEqual("CONNECTING", sm.getStateName());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.getStateId());
      Assert.AreEqual("RELEASED", sm.getStateName());

      sm.destroy();

      // Second
      sm = new CStateMachine(_manager);
      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero, sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.getStateId());

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.getStateId());
      Assert.AreEqual("INCOMING", sm.getStateName());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.getStateId());
      Assert.AreEqual("ALERTING", sm.getStateName());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.getStateId());
      Assert.AreEqual("CONNECTING", sm.getStateName());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.getStateId());
      Assert.AreEqual("RELEASED", sm.getStateName());
      sm.destroy();

      // third

      sm = new CStateMachine(_manager);
      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero, sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.getStateId());

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.getStateId());
      Assert.AreEqual("INCOMING", sm.getStateName());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.getStateId());
      Assert.AreEqual("ALERTING", sm.getStateName());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.getStateId());
      Assert.AreEqual("CONNECTING", sm.getStateName());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.getStateId());
      Assert.AreEqual("RELEASED", sm.getStateName());
      sm.destroy();
    }

    [Test]
    public void testMultipleStateMachines()
    {
      CStateMachine sm1 = new CStateMachine(_manager);
      CStateMachine sm2 = new CStateMachine(_manager);
      CStateMachine sm3 = new CStateMachine(_manager);

      Assert.AreEqual(-1, sm1.Session);
      Assert.AreEqual(TimeSpan.Zero, sm1.Duration);
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());

      Assert.AreEqual(-1, sm2.Session);
      Assert.AreEqual(TimeSpan.Zero, sm2.Duration);
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());

      Assert.AreEqual(-1, sm3.Session);
      Assert.AreEqual(TimeSpan.Zero, sm3.Duration);
      Assert.AreEqual(EStateId.IDLE, sm3.getStateId());

      // changing state
      sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      sm2.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm2.getStateId());
      sm3.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm3.getStateId());

      sm1.destroy();
      sm2.destroy();
      sm3.destroy();

      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());
      Assert.AreEqual(EStateId.IDLE, sm3.getStateId());
    }

    [Test]
    public void testMultipleStateMachinesSequence()
    {
      CStateMachine sm1 = new CStateMachine(_manager);

      Assert.AreEqual(-1, sm1.Session);
      Assert.AreEqual(TimeSpan.Zero, sm1.Duration);
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());

      // changing state
      sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      sm1.destroy();

      CStateMachine sm2 = new CStateMachine(_manager);
      Assert.AreEqual(-1, sm2.Session);
      Assert.AreEqual(TimeSpan.Zero, sm2.Duration);
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());
      
      sm2.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm2.getStateId());

      sm2.destroy();

      CStateMachine sm3 = new CStateMachine(_manager);
      Assert.AreEqual(-1, sm3.Session);
      Assert.AreEqual(TimeSpan.Zero, sm3.Duration);
      Assert.AreEqual(EStateId.IDLE, sm3.getStateId());

      sm3.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm3.getStateId());

      sm3.destroy();

      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());
      Assert.AreEqual(EStateId.IDLE, sm3.getStateId());


    }

    [Test]
    public void testIncomingCall()
    {
      CStateMachine sm1 = new CStateMachine(_manager);
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
      Assert.AreEqual(false, sm1.Incoming);
      sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);

      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.changeState(EStateId.ACTIVE);
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.destroy();
    }

    [Test]
    public void testOutgoingCall()
    {
      CStateMachine sm1 = new CStateMachine(_manager);
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
      Assert.AreEqual(false, sm1.Incoming);
      sm1.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm1.getStateId());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm1.getStateId());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.changeState(EStateId.ACTIVE);
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual("ACTIVE", sm1.getStateName());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(true, sm1.Counting);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.destroy();
    }

    [Test]
    public void testStateMachineEventHandlingOutgoing()
    {
      CStateMachine sm1 = new CStateMachine(_manager);
      sm1.getState().makeCall("1234", 0);
      Assert.AreEqual(EStateId.CONNECTING, sm1.getStateId());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNo);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.getState().onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm1.getStateId());
      Assert.AreEqual(false, sm1.Counting);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.getState().onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.getState().onReleased();
      Assert.AreEqual(EStateId.RELEASED, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());
    }

    [Test]
    public void testStateMachineEventHandlingIncoming()
    {
      CStateMachine sm1 = new CStateMachine(_manager);
      
      sm1.getState().incomingCall("1234","");
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNo);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.getState().acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.getState().onReleased();
      Assert.AreEqual(EStateId.RELEASED, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());
    }


    [Test]
    public void testCallFeaturesCallHold()
    {
      CStateMachine sm1 = new CStateMachine(_manager);

      sm1.getState().incomingCall("1234","");
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNo);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.getState().acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);

      sm1.getState().holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId()); // still ACTIVE (waiting confirmation)
      sm1.getState().onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm1.getStateId());
      // check twice hold
      sm1.getState().holdCall();
      Assert.AreEqual(EStateId.HOLDING, sm1.getStateId());

      sm1.getState().retrieveCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());

      sm1.getState().holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId()); // still ACTIVE (waiting confirmation)
      sm1.getState().onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm1.getStateId());

      sm1.destroy();
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
    }

    [Test]
    public void testCallFeaturesCallHoldConfirm()
    {
      CStateMachine inc = this.makeIncomingCallWithAnswer(1);

      // try hold confirmation without hold request
      inc.getState().onHoldConfirm();
      // no effect
      Assert.AreEqual(EStateId.ACTIVE, inc.getStateId());
      Assert.AreEqual(false, inc.HoldRequested);
      // hold request
      inc.getState().holdCall();
      Assert.AreEqual(true, inc.HoldRequested);
      // no effect
      Assert.AreEqual(EStateId.ACTIVE, inc.getStateId());
      inc.getState().onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, inc.getStateId());
      
      inc.destroy();
    }

    [Test]
    public void testCallFeaturesCallHoldMultiple()
    {
      CStateMachine sm1 = new CStateMachine(_manager);
      sm1.getState().incomingCall("1234","");
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNo);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.getState().acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);

      sm1.getState().holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId()); // still ACTIVE (waiting confirmation)
      sm1.getState().onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm1.getStateId());

      // next call
      CStateMachine sm2 = new CStateMachine(_manager);

      sm2.getState().makeCall("4444", 0);
      Assert.AreEqual(EStateId.CONNECTING, sm2.getStateId());
      Assert.AreEqual(false, sm2.Incoming);
      Assert.AreEqual("4444", sm2.CallingNo);

      sm2.getState().onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm2.getStateId());
      Assert.AreEqual(false, sm2.Counting);

      sm2.getState().onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm2.getStateId());
      Assert.AreEqual(true, sm2.Counting);

      sm2.getState().holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm2.getStateId()); // still ACTIVE (waiting confirmation)
      sm2.getState().onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm2.getStateId());

      // release first
      sm1.getState().onReleased();
      Assert.AreEqual(EStateId.RELEASED, sm1.getStateId());
      sm2.getState().onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm2.getStateId());

      sm2.getState().endCall();
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());
      sm2.getState().onReleased();
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());
    }

    [Test]
    public void testCallFeaturesCallWaiting()
    {
      // out call
      CStateMachine sm2 = new CStateMachine(_manager);

      sm2.getState().makeCall("4444", 0);
      Assert.AreEqual(EStateId.CONNECTING, sm2.getStateId());
      Assert.AreEqual(false, sm2.Incoming);
      Assert.AreEqual("4444", sm2.CallingNo);

      sm2.getState().onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm2.getStateId());
      Assert.AreEqual(false, sm2.Counting);

      sm2.getState().onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm2.getStateId());
      Assert.AreEqual(true, sm2.Counting);

      // inc call
      CStateMachine sm1 = new CStateMachine(_manager);
      sm1.getState().incomingCall("1234","");
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNo);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      // check what happens here? 
      sm1.getState().acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);
      // this should be done automatically by call manager
      // Here we do not test call manager
      //Assert.AreEqual(EStateId.HOLDING, sm2.getStateId()); 

      sm1.getState().endCall();
      sm2.getState().endCall();
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());
      sm1.getState().onReleased();
      sm2.getState().onReleased();
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());

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
      CStateMachine smOut = makeOutgoingCall();
      CStateMachine smInc = makeIncomingCall(2); // 1st call reserve sessionId 1 (nullproxy)

      // accept incoming
      _manager.onUserAnswer(smInc.Session);
      smOut.getState().onHoldConfirm();

      Assert.AreEqual(EStateId.ACTIVE, smInc.getStateId());
      Assert.AreEqual(EStateId.HOLDING, smOut.getStateId());

      smOut.getState().endCall();
      Assert.AreEqual(EStateId.IDLE, smOut.getStateId());
      smInc.getState().endCall();
      Assert.AreEqual(EStateId.IDLE, smInc.getStateId());
    
      Assert.AreEqual(0, CCallManager.getInstance().Count);
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

      CStateMachine smOut = makeOutgoingCall();
      CStateMachine smInc = makeIncomingCall(2); // 1st call reserve sessionId 1 (nullproxy)

      // accept incoming
      _manager.onUserAnswer(smInc.Session);
      smOut.getState().onHoldConfirm();

      Assert.AreEqual(EStateId.ACTIVE, smInc.getStateId());
      Assert.AreEqual(EStateId.HOLDING, smOut.getStateId());

      // Retrieve 
      _manager.onUserHoldRetrieve(smOut.Session);
      smInc.getState().onHoldConfirm();

      Assert.AreEqual(EStateId.HOLDING, smInc.getStateId());
      Assert.AreEqual(EStateId.ACTIVE, smOut.getStateId());

      smOut.getState().endCall();
      Assert.AreEqual(EStateId.IDLE, smOut.getStateId());
      smInc.getState().endCall();
      Assert.AreEqual(EStateId.IDLE, smInc.getStateId());

      Assert.AreEqual(0, CCallManager.getInstance().Count);
    }


    [Test]
    public void testCallPendingOnUserAnswer()
    {
      CStateMachine call = this.makeOutgoingCall();
      //CStateMachine inccall = _manager.createSession(2, "1234");
      _proxy.onIncomingCall(2, "1234", "");
      CStateMachine inccall = _manager.getCall(2);
      //inccall.getState().incomingCall("1234", "");
      // nothing changed yet (waiting Hold Conf)
      Assert.AreEqual(EStateId.ACTIVE, call.getStateId());
      Assert.AreEqual(EStateId.INCOMING, inccall.getStateId());

      _manager.onUserAnswer(inccall.Session); // set pending action
      // hold conf
      call.getState().onHoldConfirm();
      // states changed
      Assert.AreEqual(EStateId.HOLDING, call.getStateId());
      Assert.AreEqual(EStateId.ACTIVE, inccall.getStateId());

      call.destroy();
      inccall.destroy();
    }

    [Test]
    public void testCallPendingOnUserRetrieve()
    {
      CStateMachine call = this.makeOutgoingCall();
      call.getState().holdCall();
      call.getState().onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, call.getStateId());

      CStateMachine inccall = this.makeIncomingCallWithAnswer(2);
      Assert.AreEqual(EStateId.ACTIVE, inccall.getStateId());

      // retrieve 1st call (HOLDING)
      _manager.onUserHoldRetrieve(call.Session);
      Assert.AreEqual(EStateId.HOLDING, call.getStateId());
      Assert.AreEqual(EStateId.ACTIVE, inccall.getStateId());

      // hold conf
      inccall.getState().onHoldConfirm();
      // states changed
      Assert.AreEqual(EStateId.ACTIVE, call.getStateId());
      Assert.AreEqual(EStateId.HOLDING, inccall.getStateId());

      call.destroy();
      inccall.destroy();
    }
  }

}
