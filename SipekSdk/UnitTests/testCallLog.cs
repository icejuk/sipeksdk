using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using Sipek;
using Sipek.Common.CallControl;
using Sipek.Common;

namespace UnitTest
{

  [TestFixture]
  public class testCallLog
  {
    private CCallLog _callLogger = new CCallLog();

    [SetUp]
    public void Init()
    {
      Assert.AreEqual(0, _callLogger.Count);
    }

    [TearDown]
    public void Destroy()
    {
      _callLogger.clearAll();
    }


    [Test]
    public void testInit()
    {
      Stack<CCallRecord> list = _callLogger.getList();
      Assert.AreEqual(0, list.Count);
    }

    [Test]
    public void testCheckRecordContent()
    {
      Stack<CCallRecord> list = _callLogger.getList();
      Assert.AreEqual(0, list.Count);
      CCallRecord rec = new CCallRecord();
      rec.Count = 1;
      rec.Duration = new TimeSpan(0,0,4);
      rec.Name = "test";
      rec.Number = "1234";
      rec.Time = new DateTime(2007,7,20,11,50,45);
      rec.Type = ECallType.EDialed;
      list.Push(rec);
      Assert.AreEqual(1, list.Count);
      
      CCallRecord realrec = list.Peek();
      Assert.AreEqual(1, realrec.Count);
      Assert.AreEqual(4, realrec.Duration.Seconds);
      Assert.AreEqual("test", realrec.Name);
      Assert.AreEqual("1234", realrec.Number);
      Assert.AreEqual(2007, realrec.Time.Year);
      Assert.AreEqual(7,realrec.Time.Month);
      Assert.AreEqual(20, realrec.Time.Day);
      Assert.AreEqual(11,realrec.Time.Hour);
      Assert.AreEqual(50, realrec.Time.Minute);
      Assert.AreEqual(45, realrec.Time.Second);
      Assert.AreEqual(ECallType.EDialed, realrec.Type);
    }

    [Test]
    public void testAddCallRecords()
    {
      Stack<CCallRecord> list = _callLogger.getList();
      Assert.AreEqual(0, list.Count);

      CCallRecord rec = new CCallRecord();
      rec.Count = 1;
      rec.Duration = new TimeSpan(0, 0, 4);
      rec.Name = "test";
      rec.Number = "1234";
      rec.Time = new DateTime(2007, 7, 20, 11, 50, 45);
      rec.Type = ECallType.EDialed;

      _callLogger.addCall(rec.Type,rec.Number,rec.Name,rec.Time,rec.Duration);
      Assert.AreEqual(1, list.Count);
    }

    [Test]
    public void testClearRecords()
    {
      testAddCallRecords();
      _callLogger.addCall(ECallType.EMissed, "1111", "", new DateTime(), new TimeSpan());
      _callLogger.addCall(ECallType.EMissed, "1111", "", new DateTime(), new TimeSpan());
      _callLogger.addCall(ECallType.EMissed, "1111", "", new DateTime(), new TimeSpan());
      _callLogger.addCall(ECallType.EMissed, "1111", "", new DateTime(), new TimeSpan());

      // only two entries because of same type and number
      Assert.AreEqual(2, _callLogger.Count);
      
      _callLogger.clearAll();

      Assert.AreEqual(0, _callLogger.Count);
    }

    [Test]
    public void testRemoveCallRecord()
    {
      testAddCallRecords();
      Assert.AreEqual(1, _callLogger.Count);
      _callLogger.deleteRecord("1234", ECallType.EDialed);
      Assert.AreEqual(0, _callLogger.Count);
    }

    [Test]
    public void testDuplicateRecord()
    {
      _callLogger.addCall(ECallType.EMissed, "1111", "", new DateTime(), new TimeSpan());
      _callLogger.addCall(ECallType.EMissed, "1111", "", new DateTime(), new TimeSpan());
      _callLogger.addCall(ECallType.EMissed, "1111", "", new DateTime(), new TimeSpan());
      _callLogger.addCall(ECallType.EMissed, "1111", "", new DateTime(), new TimeSpan());
      Assert.AreEqual(1, _callLogger.Count);
    }

    [Test]
    public void testAddRecordMax()
    {
      Assert.Ignore();
    }

    [Test]
    public void testGetRecordType()
    {
      _callLogger.addCall(ECallType.EMissed, "1111", "", new DateTime(), new TimeSpan());
      _callLogger.addCall(ECallType.EDialed, "1111", "", new DateTime(), new TimeSpan());
      _callLogger.addCall(ECallType.EReceived, "1111", "", new DateTime(), new TimeSpan());
      Assert.AreEqual(3, _callLogger.Count);
      Stack<CCallRecord> rec = _callLogger.getList(ECallType.EDialed);
      Assert.AreEqual(1, rec.Count);

      Stack<CCallRecord> rec1 = _callLogger.getList(ECallType.EReceived);
      Assert.AreEqual(1, rec1.Count);

      Stack<CCallRecord> rec2 = _callLogger.getList(ECallType.EMissed);
      Assert.AreEqual(1, rec2.Count);

      Stack<CCallRecord> rec3 = _callLogger.getList(ECallType.EAll);
      Assert.AreEqual(3, rec3.Count);
    }


  }

}
