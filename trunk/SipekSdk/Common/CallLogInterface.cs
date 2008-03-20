using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{

  public enum ECallType : int
  {
    EDialed,
    EReceived,
    EMissed,
    EAll,
    EUndefined
  }

  /// <summary>
  /// 
  /// </summary>
  public interface ICallLogInterface
  {
    // CallControl interface
    void addCall(ECallType type, string number, string name, System.DateTime time, System.TimeSpan duration);

    // GUI interface
    void save();

    Stack<CCallRecord> getList();
    Stack<CCallRecord> getList(ECallType type);

    void deleteRecord(CCallRecord record);
  }



  /// <summary>
  /// 
  /// </summary>
  public class CCallRecord
  {
    private ECallType _type;
    private string _name = "";
    private string _number = "";
    private DateTime _time;
    private TimeSpan _duration;
    private int _count;

    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }
    public string Number
    {
      get { return _number; }
      set { _number = value; }
    }
    public ECallType Type
    {
      get { return _type; }
      set { _type = value; }
    }
    public TimeSpan Duration
    {
      get { return _duration; }
      set { _duration = value; }
    }
    public DateTime Time
    {
      get { return _time; }
      set { _time = value; }
    }
    public int Count
    {
      get { return _count; }
      set { _count = value; }
    }
  }


  /// <summary>
  /// 
  /// </summary>
  public class CNullCallLog : ICallLogInterface
  {
    public void addCall(ECallType type, string number, string name, System.DateTime time, System.TimeSpan duration) { }

    public void save() { }
    public Stack<CCallRecord> getList() { return null; }
    public Stack<CCallRecord> getList(ECallType type) { return null; }
    public void deleteRecord(CCallRecord record) { }
  }

}
