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

#if DEBUG

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

    public int makeCallByUri(string uri)
    {
      throw new Exception("The method or operation is not implemented.");
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

    public void onIncomingCall(int sessionId, string number, string info)
    {
      BaseIncomingCall(sessionId, number, info);
    }

    public override ICallProxyInterface createCallProxy()
    {
      return new NullCallProxy();
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
    public IStateMachine createStateMachine(CCallManager mng)
    {
      return new CStateMachine(mng);
    }

    public IMediaProxyInterface MediaProxy
    {
      get
      {
        return _mediaproxy;
      }
      set
      {
        ;
      }
    }

    public ICallLogInterface CallLogger
    {
      get
      {
        return _calllogger;
      }
      set
      {
        ;
      }
    }

    public IConfiguratorInterface Configurator
    {
      get
      {
        return _config;
      }
      set
      {
        ;
      }
    }

    IVoipProxy AbstractFactory.CommonProxy
    {
      get
      {
        return _proxy;
      }
      set
      {
        ;
      }
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
    private IStateMachine makeOutgoingCall()
    {
      IStateMachine sm1 = _manager.createOutboundCall("1234");

      Assert.AreEqual(EStateId.CONNECTING, sm1.State.Id);
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.State.onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm1.State.Id);
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.State.onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id);
      Assert.AreEqual("ACTIVE", sm1.State.ToString());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(true, sm1.Counting);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      return sm1;
    }

    IStateMachine makeIncomingCall(int sessionId)
    {
      string number = "1234";
      //IStateMachine sm1 = new CStateMachine(null);
      //IStateMachine sm1 = _manager.createOutboundCall(sessionId, number);
      _proxy.onIncomingCall(sessionId, number, "");
      IStateMachine sm1 = _manager.getCall(sessionId);
      //sm1.State.incomingCall(number,"");

      //sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.State.Id);
      Assert.AreEqual(true, sm1.Incoming);

      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);
      
      return sm1;
    }

    IStateMachine makeIncomingCallWithAnswer(int sessionId)
    {
      string number = "1234";
      //IStateMachine sm1 = new CStateMachine(null);
      //IStateMachine sm1 = _manager.createSession(sessionId, number);
      _proxy.onIncomingCall(sessionId, number, "");
      IStateMachine sm1 = _manager.getCall(sessionId);
      //sm1.State.incomingCall(number, "");

      //sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.State.Id);
      Assert.AreEqual(true, sm1.Incoming);

      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      _manager.onUserAnswer(sm1.Session);
      //sm1.State.acceptCall(sm1.Session);
      //sm1.changeState(EStateId.ACTIVE);
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id);
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      return sm1;
    }

    [Test]
    public void testStateMachineCreate()
    {
      IStateMachine sm = new CStateMachine(_manager);

      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero ,sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.State.Id);

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.State.Id);
      Assert.AreEqual("INCOMING", sm.State.ToString());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.State.Id);
      Assert.AreEqual("ALERTING", sm.State.ToString());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.State.Id);
      Assert.AreEqual("CONNECTING", sm.State.ToString());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.State.Id);
      Assert.AreEqual("RELEASED", sm.State.ToString());

      sm.destroy();
  
    }

    [Test]
    public void testStateMachineCreateSequence()
    {
      IStateMachine sm = new CStateMachine(_manager);

      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero, sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.State.Id);

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.State.Id);
      Assert.AreEqual("INCOMING", sm.State.ToString());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.State.Id);
      Assert.AreEqual("ALERTING", sm.State.ToString());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.State.Id);
      Assert.AreEqual("CONNECTING", sm.State.ToString());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.State.Id);
      Assert.AreEqual("RELEASED", sm.State.ToString());

      sm.destroy();

      // Second
      sm = new CStateMachine(_manager);
      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero, sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.State.Id);

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.State.Id);
      Assert.AreEqual("INCOMING", sm.State.ToString());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.State.Id);
      Assert.AreEqual("ALERTING", sm.State.ToString());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.State.Id);
      Assert.AreEqual("CONNECTING", sm.State.ToString());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.State.Id);
      Assert.AreEqual("RELEASED", sm.State.ToString());
      sm.destroy();

      // third

      sm = new CStateMachine(_manager);
      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero, sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.State.Id);

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.State.Id);
      Assert.AreEqual("INCOMING", sm.State.ToString());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.State.Id);
      Assert.AreEqual("ALERTING", sm.State.ToString());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.State.Id);
      Assert.AreEqual("CONNECTING", sm.State.ToString());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.State.Id);
      Assert.AreEqual("RELEASED", sm.State.ToString());
      sm.destroy();
    }

    [Test]
    public void testMultipleStateMachines()
    {
      IStateMachine sm1 = new CStateMachine(_manager);
      IStateMachine sm2 = new CStateMachine(_manager);
      IStateMachine sm3 = new CStateMachine(_manager);

      Assert.AreEqual(-1, sm1.Session);
      Assert.AreEqual(TimeSpan.Zero, sm1.Duration);
      Assert.AreEqual(EStateId.IDLE, sm1.State.Id);

      Assert.AreEqual(-1, sm2.Session);
      Assert.AreEqual(TimeSpan.Zero, sm2.Duration);
      Assert.AreEqual(EStateId.IDLE, sm2.State.Id);

      Assert.AreEqual(-1, sm3.Session);
      Assert.AreEqual(TimeSpan.Zero, sm3.Duration);
      Assert.AreEqual(EStateId.IDLE, sm3.State.Id);

      // changing state
      sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.State.Id);
      sm2.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm2.State.Id);
      sm3.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm3.State.Id);

      sm1.destroy();
      sm2.destroy();
      sm3.destroy();

      Assert.AreEqual(EStateId.IDLE, sm1.State.Id);
      Assert.AreEqual(EStateId.IDLE, sm2.State.Id);
      Assert.AreEqual(EStateId.IDLE, sm3.State.Id);
    }

    [Test]
    public void testMultipleStateMachinesSequence()
    {
      IStateMachine sm1 = new CStateMachine(_manager);

      Assert.AreEqual(-1, sm1.Session);
      Assert.AreEqual(TimeSpan.Zero, sm1.Duration);
      Assert.AreEqual(EStateId.IDLE, sm1.State.Id);

      // changing state
      sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.State.Id);
      sm1.destroy();

      IStateMachine sm2 = new CStateMachine(_manager);
      Assert.AreEqual(-1, sm2.Session);
      Assert.AreEqual(TimeSpan.Zero, sm2.Duration);
      Assert.AreEqual(EStateId.IDLE, sm2.State.Id);
      
      sm2.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm2.State.Id);

      sm2.destroy();

      IStateMachine sm3 = new CStateMachine(_manager);
      Assert.AreEqual(-1, sm3.Session);
      Assert.AreEqual(TimeSpan.Zero, sm3.Duration);
      Assert.AreEqual(EStateId.IDLE, sm3.State.Id);

      sm3.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm3.State.Id);

      sm3.destroy();

      Assert.AreEqual(EStateId.IDLE, sm1.State.Id);
      Assert.AreEqual(EStateId.IDLE, sm2.State.Id);
      Assert.AreEqual(EStateId.IDLE, sm3.State.Id);


    }

    [Test]
    public void testIncomingCall()
    {
      IStateMachine sm1 = new CStateMachine(_manager);
      Assert.AreEqual(EStateId.IDLE, sm1.State.Id);
      Assert.AreEqual(false, sm1.Incoming);
      sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.State.Id);
      Assert.AreEqual(true, sm1.Incoming);

      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.changeState(EStateId.ACTIVE);
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id);
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.destroy();
    }

    [Test]
    public void testOutgoingCall()
    {
      IStateMachine sm1 = new CStateMachine(_manager);
      Assert.AreEqual(EStateId.IDLE, sm1.State.Id);
      Assert.AreEqual(false, sm1.Incoming);
      sm1.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm1.State.Id);
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm1.State.Id);
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.changeState(EStateId.ACTIVE);
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id);
      Assert.AreEqual("ACTIVE", sm1.State.ToString());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(true, sm1.Counting);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.destroy();
    }

    [Test]
    public void testStateMachineEventHandlingOutgoing()
    {
      IStateMachine sm1 = new CStateMachine(_manager);
      sm1.State.makeCall("1234", 0);
      Assert.AreEqual(EStateId.CONNECTING, sm1.State.Id);
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNumber);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.State.onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm1.State.Id);
      Assert.AreEqual(false, sm1.Counting);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.State.onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id);
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.State.onReleased();
      Assert.AreEqual(EStateId.RELEASED, sm1.State.Id);
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());
    }

    [Test]
    public void testStateMachineEventHandlingIncoming()
    {
      IStateMachine sm1 = new CStateMachine(_manager);
      
      sm1.State.incomingCall("1234","");
      Assert.AreEqual(EStateId.INCOMING, sm1.State.Id);
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNumber);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.State.acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id);
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.State.onReleased();
      Assert.AreEqual(EStateId.RELEASED, sm1.State.Id);
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());
    }


    [Test]
    public void testCallFeaturesCallHold()
    {
      IStateMachine sm1 = new CStateMachine(_manager);

      sm1.State.incomingCall("1234","");
      Assert.AreEqual(EStateId.INCOMING, sm1.State.Id);
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNumber);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.State.acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id);
      Assert.AreEqual(true, sm1.Counting);

      sm1.State.holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id); // still ACTIVE (waiting confirmation)
      sm1.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm1.State.Id);
      // check twice hold
      sm1.State.holdCall();
      Assert.AreEqual(EStateId.HOLDING, sm1.State.Id);

      sm1.State.retrieveCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id);

      sm1.State.holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id); // still ACTIVE (waiting confirmation)
      sm1.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm1.State.Id);

      sm1.destroy();
      Assert.AreEqual(EStateId.IDLE, sm1.State.Id);
    }

    [Test]
    public void testCallFeaturesCallHoldConfirm()
    {
      IStateMachine inc = this.makeIncomingCallWithAnswer(1);

      // try hold confirmation without hold request
      inc.State.onHoldConfirm();
      // no effect
      Assert.AreEqual(EStateId.ACTIVE, inc.State.Id);
      Assert.AreEqual(false, inc.HoldRequested);
      // hold request
      inc.State.holdCall();
      Assert.AreEqual(true, inc.HoldRequested);
      // no effect
      Assert.AreEqual(EStateId.ACTIVE, inc.State.Id);
      inc.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, inc.State.Id);
      
      inc.destroy();
    }

    [Test]
    public void testCallFeaturesCallHoldMultiple()
    {
      IStateMachine sm1 = new CStateMachine(_manager);
      sm1.State.incomingCall("1234","");
      Assert.AreEqual(EStateId.INCOMING, sm1.State.Id);
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNumber);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.State.acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id);
      Assert.AreEqual(true, sm1.Counting);

      sm1.State.holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id); // still ACTIVE (waiting confirmation)
      sm1.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm1.State.Id);

      // next call
      IStateMachine sm2 = new CStateMachine(_manager);

      sm2.State.makeCall("4444", 0);
      Assert.AreEqual(EStateId.CONNECTING, sm2.State.Id);
      Assert.AreEqual(false, sm2.Incoming);
      Assert.AreEqual("4444", sm2.CallingNumber);

      sm2.State.onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm2.State.Id);
      Assert.AreEqual(false, sm2.Counting);

      sm2.State.onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm2.State.Id);
      Assert.AreEqual(true, sm2.Counting);

      sm2.State.holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm2.State.Id); // still ACTIVE (waiting confirmation)
      sm2.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm2.State.Id);

      // release first
      sm1.State.onReleased();
      Assert.AreEqual(EStateId.RELEASED, sm1.State.Id);
      sm2.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm2.State.Id);

      sm2.State.endCall();
      Assert.AreEqual(EStateId.IDLE, sm2.State.Id);
      sm2.State.onReleased();
      Assert.AreEqual(EStateId.IDLE, sm2.State.Id);
    }

    [Test]
    public void testCallFeaturesCallWaiting()
    {
      // out call
      IStateMachine sm2 = new CStateMachine(_manager);

      sm2.State.makeCall("4444", 0);
      Assert.AreEqual(EStateId.CONNECTING, sm2.State.Id);
      Assert.AreEqual(false, sm2.Incoming);
      Assert.AreEqual("4444", sm2.CallingNumber);

      sm2.State.onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm2.State.Id);
      Assert.AreEqual(false, sm2.Counting);

      sm2.State.onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm2.State.Id);
      Assert.AreEqual(true, sm2.Counting);

      // inc call
      IStateMachine sm1 = new CStateMachine(_manager);
      sm1.State.incomingCall("1234","");
      Assert.AreEqual(EStateId.INCOMING, sm1.State.Id);
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNumber);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      // check what happens here? 
      sm1.State.acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.State.Id);
      Assert.AreEqual(true, sm1.Counting);
      // this should be done automatically by call manager
      // Here we do not test call manager
      //Assert.AreEqual(EStateId.HOLDING, sm2.State.Id); 

      sm1.State.endCall();
      sm2.State.endCall();
      Assert.AreEqual(EStateId.IDLE, sm1.State.Id);
      Assert.AreEqual(EStateId.IDLE, sm2.State.Id);
      sm1.State.onReleased();
      sm2.State.onReleased();
      Assert.AreEqual(EStateId.IDLE, sm1.State.Id);
      Assert.AreEqual(EStateId.IDLE, sm2.State.Id);

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

      Assert.AreEqual(EStateId.ACTIVE, smInc.State.Id);
      Assert.AreEqual(EStateId.HOLDING, smOut.State.Id);

      smOut.State.endCall();
      Assert.AreEqual(EStateId.IDLE, smOut.State.Id);
      smInc.State.endCall();
      Assert.AreEqual(EStateId.IDLE, smInc.State.Id);
    
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

      IStateMachine smOut = makeOutgoingCall();
      IStateMachine smInc = makeIncomingCall(2); // 1st call reserve sessionId 1 (nullproxy)

      // accept incoming
      _manager.onUserAnswer(smInc.Session);
      smOut.State.onHoldConfirm();

      Assert.AreEqual(EStateId.ACTIVE, smInc.State.Id);
      Assert.AreEqual(EStateId.HOLDING, smOut.State.Id);

      // Retrieve 
      _manager.onUserHoldRetrieve(smOut.Session);
      smInc.State.onHoldConfirm();

      Assert.AreEqual(EStateId.HOLDING, smInc.State.Id);
      Assert.AreEqual(EStateId.ACTIVE, smOut.State.Id);

      smOut.State.endCall();
      Assert.AreEqual(EStateId.IDLE, smOut.State.Id);
      smInc.State.endCall();
      Assert.AreEqual(EStateId.IDLE, smInc.State.Id);

      Assert.AreEqual(0, CCallManager.getInstance().Count);
    }


    [Test]
    public void testCallPendingOnUserAnswer()
    {
      IStateMachine call = this.makeOutgoingCall();
      //IStateMachine inccall = _manager.createSession(2, "1234");
      _proxy.onIncomingCall(2, "1234", "");
      IStateMachine inccall = _manager.getCall(2);
      //inccall.State.incomingCall("1234", "");
      // nothing changed yet (waiting Hold Conf)
      Assert.AreEqual(EStateId.ACTIVE, call.State.Id);
      Assert.AreEqual(EStateId.INCOMING, inccall.State.Id);

      _manager.onUserAnswer(inccall.Session); // set pending action
      // hold conf
      call.State.onHoldConfirm();
      // states changed
      Assert.AreEqual(EStateId.HOLDING, call.State.Id);
      Assert.AreEqual(EStateId.ACTIVE, inccall.State.Id);

      call.destroy();
      inccall.destroy();
    }

    [Test]
    public void testCallPendingOnUserRetrieve()
    {
      IStateMachine call = this.makeOutgoingCall();
      call.State.holdCall();
      call.State.onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, call.State.Id);

      IStateMachine inccall = this.makeIncomingCallWithAnswer(2);
      Assert.AreEqual(EStateId.ACTIVE, inccall.State.Id);

      // retrieve 1st call (HOLDING)
      _manager.onUserHoldRetrieve(call.Session);
      Assert.AreEqual(EStateId.HOLDING, call.State.Id);
      Assert.AreEqual(EStateId.ACTIVE, inccall.State.Id);

      // hold conf
      inccall.State.onHoldConfirm();
      // states changed
      Assert.AreEqual(EStateId.ACTIVE, call.State.Id);
      Assert.AreEqual(EStateId.HOLDING, inccall.State.Id);

      call.destroy();
      inccall.destroy();
    }
  }
#endif
}
