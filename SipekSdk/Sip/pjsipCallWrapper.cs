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
 * 
 * @see http://sipekphone.googlepages.com/pjsipwrapper
 * @see http://voipengine.googlepages.com/
 * 
 */

using System;
using System.Text;
using Sipek.Common;
using System.Runtime.InteropServices;

namespace Sipek.Sip
{
  #region Sip Call Proxy
  /// <summary>
  /// Implementation of call proxy. Each call (session) contains an instance of a call proxy. 
  /// SipCallProxy passes requests further to pjsip stack 
  /// </summary>
  public class CSipCallProxy : ICallProxyInterface
  {
    #region DLL declarations
    // call API
    [DllImport("pjsipDll.dll")]
    private static extern int dll_makeCall(int accountId, string uri);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_releaseCall(int callId);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_answerCall(int callId, int code);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_holdCall(int callId);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_retrieveCall(int callId);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_xferCall(int callId, string uri);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_xferCallWithReplaces(int callId, int dstSession);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_serviceReq(int callId, int serviceCode, string destUri);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_dialDtmf(int callId, string digits, int mode);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_sendInfo(int callid, string content);

    #endregion

    #region Properties
    private IConfiguratorInterface _config = new NullConfigurator();

    public IConfiguratorInterface Config
    {
      get { return _config; }
    }

    private int _sessionId;
    public int SessionId
    {
      get { return _sessionId; }
      set { _sessionId = value; }
    }
    #endregion

    #region Constructor

    public CSipCallProxy(IConfiguratorInterface config)
    {
      _config = config;
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Method makeCall creates call session. Checks the 1st parameter 
    /// format is SIP URI, if not build one.  
    /// </summary>
    /// <param name="dialedNo"></param>
    /// <param name="accountId"></param>
    /// <returns>SessionId chosen by pjsip stack</returns>
    public int makeCall(string dialedNo, int accountId)
    {
      string sipuri = "";

      // check if call by URI
      if (dialedNo.Contains("sip:"))
      {
        // do nothing...
        sipuri = dialedNo;
      }
      else
      {
        // prepare URI
        sipuri = "sip:" + dialedNo + "@" + Config.getAccount(accountId).HostName;
      }
      // Store session identification for further requests
      SessionId = dll_makeCall(accountId, sipuri);
      return SessionId;
    }

    /// <summary>
    /// End call for a given session
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public bool endCall()
    {
      dll_releaseCall(SessionId);
      return true;
    }

    /// <summary>
    /// Signals sip stack that device is alerted (ringing)
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public bool alerted()
    {
      dll_answerCall(SessionId, 180);
      return true;
    }

    /// <summary>
    /// Signals that user accepts the call (asnwer)
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public bool acceptCall()
    {
      dll_answerCall(SessionId, 200);
      return true;
    }

    /// <summary>
    /// Hold request for a given session
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public bool holdCall()
    {
      dll_holdCall(SessionId);
      return true;
    }

    /// <summary>
    /// Retrieve request for a given session
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public bool retrieveCall()
    {
      dll_retrieveCall(SessionId);
      return true;
    }

    /// <summary>
    /// Trasfer call to number
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="number"></param>
    /// <returns></returns>
    public bool xferCall(string number)
    {
      string uri = "sip:" + number + "@" + Config.getAccount(Config.DefaultAccountIndex).HostName;
      dll_xferCall(SessionId, uri);
      return true;
    }

    /// <summary>
    /// Transfer call to other session
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public bool xferCallSession(int session)
    {
      dll_xferCallWithReplaces(SessionId, session);
      return true;
    }

    /// <summary>
    /// Make conference with given session
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public bool threePtyCall(int session)
    {
      dll_serviceReq(SessionId, (int)EServiceCodes.SC_3PTY, "");
      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="code"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    public bool serviceRequest(int code, string dest)
    {
      string destUri = "<sip:" + dest + "@" + Config.getAccount(Config.DefaultAccountIndex).HostName + ">";
      dll_serviceReq(SessionId, (int)code, destUri);
      return true;
    }

    /// <summary>
    /// Send dtmf digit
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="digits"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public bool dialDtmf(string digits, int mode)
    {
      // TODO :::check the dtmf mode
      if (mode == 0)
      {
        dll_dialDtmf(SessionId, digits, mode);
      }
      else
      {
        dll_sendInfo(SessionId, digits);
      }
      return true;
    }

    #endregion Methods
  }

  #endregion
}
