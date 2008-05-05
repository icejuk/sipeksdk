/* 
 * Copyright (C) 2008 Sasa Coh <sasacoh@gmail.com>
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
 * 
 * @see http://sipekphone.googlepages.com/pjsipwrapper
 * @see http://voipengine.googlepages.com/
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using Sipek.Common;
using System.Runtime.InteropServices;

namespace Sipek.Sip
{
  
  public class pjsipPresenceAndMessaging : IPresenceAndMessaging
  {
    #region Dll declarations
    [DllImport("pjsipDll.dll")]
    private static extern int dll_addBuddy(string uri, bool subscribe);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_removeBuddy(int buddyId);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_sendMessage(int buddyId, string uri, string message);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_setStatus(int accId, int presence_state);
    #endregion 

    #region Callback declarations
    delegate int OnMessageReceivedCallback(StringBuilder from, StringBuilder message);
    delegate int OnBuddyStatusChangedCallback(int buddyId, int status, StringBuilder statusText);

    [DllImport("pjsipDll.dll")]
    private static extern int onMessageReceivedCallback(OnMessageReceivedCallback cb);
    [DllImport("pjsipDll.dll")]
    private static extern int onBuddyStatusChangedCallback(OnBuddyStatusChangedCallback cb);

    static OnMessageReceivedCallback mrdel = new OnMessageReceivedCallback(onMessageReceived);
    static OnBuddyStatusChangedCallback bscdel = new OnBuddyStatusChangedCallback(onBuddyStatusChanged);
    
    #endregion

    #region Constructor
    static private pjsipPresenceAndMessaging _instance = null;
    static public pjsipPresenceAndMessaging Instance    
    {
      get 
      { 
        if (_instance == null) _instance = new pjsipPresenceAndMessaging();
        return _instance;
      }
    }

    private pjsipPresenceAndMessaging()
    {
      onBuddyStatusChangedCallback(bscdel);
      onMessageReceivedCallback(mrdel);
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Add new entry in a buddy list and subscribe presence
    /// </summary>
    /// <param name="ident">Buddy address (without hostname part</param>
    /// <param name="presence">subscribe presence flag</param>
    /// <returns></returns>
    public override int addBuddy(string name, bool presence, int accId)
    {
       string sipuri = "";

      // check if name contains URI
      if (name.Contains("sip:"))
      {
        // do nothing...
        sipuri = name;
      }
      else
      { 
        sipuri = "sip:" + name + "@" + Config.Accounts[accId].HostName;
      }
      return dll_addBuddy(sipuri, presence);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buddyId"></param>
    /// <returns></returns>
    public override int delBuddy(int buddyId)
    {
      return dll_removeBuddy(buddyId);
    }

    /// <summary>
    /// Send an instance message
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public override int sendMessage(string destAddress, string message, int accId)
    {
       string sipuri = "";

      // check if name contains URI
       if (destAddress.Contains("sip:"))
      {
        // do nothing...
        sipuri = destAddress;
      }
      else
      {
        sipuri = "sip:" + destAddress + "@" + Config.Accounts[accId].HostName;
      }
      return dll_sendMessage(accId, sipuri, message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public override int sendMessage(string destAddress, string message)
    {
      return sendMessage(destAddress, message, Config.DefaultAccountIndex);
    }

    /// <summary>
    /// Set presence status
    /// </summary>
    /// <param name="accId"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public override int setStatus(int accId, EUserStatus status)
    {
      return dll_setStatus(accId, (int)status);
    }

    #endregion

    #region Callbacks
    private static int onMessageReceived(StringBuilder from, StringBuilder text)
    {
      Instance.BaseMessageReceived(from.ToString(), text.ToString());
      return 1;
    }

    private static int onBuddyStatusChanged(int buddyId, int status, StringBuilder text)
    {
      Instance.BaseBuddyStatusChanged(buddyId, status, text.ToString());
      return 1;
    }
    #endregion
  }
}
