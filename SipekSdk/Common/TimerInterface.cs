using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{
  public delegate void TimerExpiredCallback(object sender, EventArgs e);

  public interface ITimer
  {
    void Start();
    void Stop();

    int Interval { get; set;}

    TimerExpiredCallback Elapsed { set;}

  }


  #region Null Pattern
  /// <summary>
  /// 
  /// </summary>
  public class NullTimer : ITimer
  {
    #region ITimer Members
    public void Start() { }
    public void Stop() { }
    public int Interval
    {
      get { return 100; }
      set { }
    }

    public TimerExpiredCallback Elapsed
    {
      set { }
    }
    #endregion
  }

#endregion
}
