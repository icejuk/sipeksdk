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
  public class pjsipRegistrar : IRegistrar
  {

    #region Dll declarations

#if LINUX
		internal const string PJSIP_DLL = "libpjsipDll.so"; 
#else
    internal const string PJSIP_DLL = "pjsipDll.dll";
#endif

    [DllImport(PJSIP_DLL)]
    private static extern int dll_registerAccount(string uri, string reguri, string domain, string username, string password, string proxy, bool isdefault);
    [DllImport(PJSIP_DLL)]
    private static extern int dll_removeAccounts();
    [DllImport(PJSIP_DLL)]
    private static extern int onRegStateCallback(OnRegStateChanged cb);
    
    #endregion

    #region Constructor
    static private pjsipRegistrar _instance = null;
    static public pjsipRegistrar Instance    
    {
      get 
      { 
        if (_instance == null) _instance = new pjsipRegistrar();
        return _instance;
      }
    }

    private pjsipRegistrar()
    {
      onRegStateCallback(rsDel);
    }
    #endregion

    #region Callback declarations
    // registration state change delegate
    delegate int OnRegStateChanged(int accountId, int regState);

    static OnRegStateChanged rsDel = new OnRegStateChanged(onRegStateChanged);
    #endregion

    #region Public methods
    /////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Register all configured accounts 
    /// </summary>
    /// <returns></returns>
    public override int registerAccounts()
    {
      //if (!IsInitialized) return -1;

      if (Config.Accounts.Count <= 0) return 0;

      // unregister accounts
      dll_removeAccounts();

      // iterate all accounts
      for (int i = 0; i < Config.Accounts.Count; i++)
      {
        IAccount acc = Config.Accounts[i];
        // check if accounts available
        if (null == acc) return -1;

        // reset account state
        BaseAccountStateChanged(i, 0);

        if (acc.Id.Length > 0)
        {
          if (acc.HostName == "0") continue;

          string displayName = acc.DisplayName;
          // warning:::Publish do not work if display name in uri !!!
          string uri = "sip:" + acc.UserName;
          if (!acc.UserName.Contains("@"))
          {
            uri += "@" + acc.HostName;
          }
          string reguri = "sip:" + acc.HostName;

          string domain = acc.DomainName;
          string username = acc.UserName;
          string password = acc.Password;

          string proxy = "";
          if (acc.ProxyAddress.Length > 0)
          {
            proxy = "sip:" + acc.ProxyAddress;
          }

          dll_registerAccount(uri, reguri, domain, username, password, proxy, (i == Config.DefaultAccountIndex ? true : false));

          // todo:::check if accId corresponds to account index!!!
        }
      }
      return 1;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="accId"></param>
    /// <param name="regState"></param>
    /// <returns></returns>
    private static int onRegStateChanged(int accId, int regState)
    {
      Instance.Config.Accounts[accId].RegState = regState;
      Instance.BaseAccountStateChanged(accId, regState);
      return 1;
    }
    #endregion
  }
}
