using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{
  public enum ETones : int
  {
    EToneDial = 0,
    EToneCongestion,
    EToneRingback,
    EToneRing,
  }

  /// <summary>
  /// 
  /// </summary>
  public interface IMediaProxyInterface
  {

    int playTone(ETones toneId);

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
