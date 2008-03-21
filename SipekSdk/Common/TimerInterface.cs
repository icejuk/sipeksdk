using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{
  /// <summary>
  /// Timer expiration callback
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  public delegate void TimerExpiredCallback(object sender, EventArgs e);

  /// <summary>
  /// Timer interface
  /// </summary>
  public interface ITimer
  {
    /// <summary>
    /// Request timer start
    /// </summary>
    void Start();

    /// <summary>
    /// Request timer stop
    /// </summary>
    void Stop();

    /// <summary>
    /// Set tiemr interval
    /// </summary>
    int Interval { get; set;}

    /// <summary>
    /// Set timer expiry callback method
    /// </summary>
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
