using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{

  /// <summary>
  /// Tone modes
  /// </summary>
  public enum ETones : int
  {
    EToneDial = 0,
    EToneCongestion,
    EToneRingback,
    EToneRing,
  }

  /// <summary>
  /// Media proxy interface for playing tones (todo recording)
  /// </summary>
  public interface IMediaProxyInterface
  {
    /// <summary>
    /// Play give tone 
    /// </summary>
    /// <param name="toneId">tone identification</param>
    /// <returns></returns>
    int playTone(ETones toneId);

    /// <summary>
    /// Stop tone
    /// </summary>
    /// <returns></returns>
    int stopTone();
  }

  #region Null Pattern

  public class NullMediaProxy : IMediaProxyInterface
  {
    #region IMediaProxyInterface Members

    public int playTone(ETones toneId)
    {
      return 1;
    }

    public int stopTone()
    {
      return 1;
    }
    #endregion
  }  
  #endregion


}
