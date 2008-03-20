using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{

  #region Enums

  public enum EUserStatus : int
  {
    AVAILABLE,
    BUSY,
    OTP,
    IDLE,
    AWAY,
    BRB,
    OFFLINE,
    OPT_MAX
  }

  public enum EServiceCodes : int
  {
    SC_CD,
    SC_CFU,
    SC_CFNR,
    SC_DND,
    SC_3PTY
  }


  public enum ECallNotification : int
  {
    CN_HOLDCONFIRM
  }

  #endregion

}
