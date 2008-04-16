/* 
 * Copyright (C) 2007 Sasa Coh <sasacoh@gmail.com>
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

using System.Runtime.InteropServices;
using System;
using System.Text;
using Sipek.Common;

namespace Sipek.Sip
{

  /// <summary>
  /// Implementation of non-call oriented VoIP interface. 
  /// This proxy is used for sip stack initialization and shut down, registration, and 
  /// callback methods handling.
  /// </summary>
  public class CSipCommonProxy : IVoipProxy
  {
    #region Constructor

    private static CSipCommonProxy _instance = null;

    public static CSipCommonProxy GetInstance()
    { 
      if (_instance == null)
      {
        _instance = new CSipCommonProxy();
      }
      return _instance;
    }

    protected CSipCommonProxy()
    {
    }
    #endregion

    #region Properties

    private bool _initialized = false;
    public override bool IsInitialized
    {
      get { return _initialized; }
      set { _initialized = value; }
    }
    #endregion

    #region Wrapper functions
    
    [DllImport("pjsipDll.dll")]
    private static extern int dll_init(SipConfigStruct config);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_main();
    [DllImport("pjsipDll.dll")]
    private static extern int dll_shutdown();
    [DllImport("pjsipDll.dll")]
    private static extern int dll_registerAccount(string uri, string reguri, string domain, string username, string password, string proxy, bool isdefault);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_addBuddy(string uri, bool subscribe);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_removeBuddy(int buddyId);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_sendMessage(int buddyId, string uri, string message);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_setStatus(int accId, int presence_state);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_removeAccounts();
    [DllImport("pjsipDll.dll")]
    private static extern int dll_getCodec(int index, StringBuilder codec);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_getNumOfCodecs();
    [DllImport("pjsipDll.dll")]
    private static extern int dll_setCodecPriority(string name, int prio);

    // Callback function registration declarations 
    // callback delegates
    delegate int OnRegStateChanged(int accountId, int regState);
    delegate int OnCallStateChanged(int callId, int stateId);
    delegate int OnCallIncoming(int callId, StringBuilder number);
    delegate int OnCallHoldConfirm(int callId);
    delegate int OnMessageReceivedCallback(StringBuilder from, StringBuilder message);
    delegate int OnBuddyStatusChangedCallback(int buddyId, int status, StringBuilder statusText);
    delegate int OnDtmfDigitCallback(int callId, int digit);
    delegate int OnMessageWaitingCallback(int mwi, StringBuilder info);

    // passing delegates to unmanaged code (.dll)
    [DllImport("pjsipDll.dll")]
    private static extern int onCallStateCallback(OnCallStateChanged cb);
    [DllImport("pjsipDll.dll")]
    private static extern int onRegStateCallback(OnRegStateChanged cb);
    [DllImport("pjsipDll.dll")]
    private static extern int onCallIncoming(OnCallIncoming cb);
    [DllImport("pjsipDll.dll")]
    private static extern int onCallHoldConfirmCallback(OnCallHoldConfirm cb);
    [DllImport("pjsipDll.dll")]
    private static extern int onMessageReceivedCallback(OnMessageReceivedCallback cb);
    [DllImport("pjsipDll.dll")]
    private static extern int onBuddyStatusChangedCallback(OnBuddyStatusChangedCallback cb);
    [DllImport("pjsipDll.dll")]
    private static extern int onDtmfDigitCallback(OnDtmfDigitCallback cb);
    [DllImport("pjsipDll.dll")]
    private static extern int onMessageWaitingCallback(OnMessageWaitingCallback cb);

    #endregion Wrapper functions

    #region Variables

    // config structure (used for special configuration options)
    public SipConfigStruct SipConfigMore = new SipConfigStruct();

    // Static declaration because of CallbackonCollectedDelegate exception!
    static OnCallStateChanged csDel = new OnCallStateChanged(onCallStateChanged);
    static OnRegStateChanged rsDel = new OnRegStateChanged(onRegStateChanged);
    static OnCallIncoming ciDel = new OnCallIncoming(onCallIncoming);
    static OnCallHoldConfirm chDel = new OnCallHoldConfirm(onCallHoldConfirm);
    static OnMessageReceivedCallback mrdel = new OnMessageReceivedCallback(onMessageReceived);
    static OnBuddyStatusChangedCallback bscdel = new OnBuddyStatusChangedCallback(onBuddyStatusChanged);
    static OnDtmfDigitCallback dtdel = new OnDtmfDigitCallback(onDtmfDigitCallback);
    static OnMessageWaitingCallback mwidel = new OnMessageWaitingCallback(onMessageWaitingCallback);


    #endregion Variables

    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private int start()
    {
      int status = -1;

      // prepare configuration struct
      // read data from Config interface. If null read all values directly from SipConfigMore
      if (!Config.IsNull)
      {
        SipConfigMore.listenPort = Config.SIPPort;
      }

      status = dll_init(SipConfigMore);

      if (status != 0) return status;

      status |= dll_main();
      return status;
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Initialize pjsip stack
    /// </summary>
    /// <returns></returns>
    public override int initialize()
    {
      // register callbacks (delegates)
      onCallIncoming( ciDel );
      onCallStateCallback( csDel );
      onRegStateCallback( rsDel );
      onCallHoldConfirmCallback(chDel);
      onBuddyStatusChangedCallback(bscdel);
      onMessageReceivedCallback(mrdel);
      onDtmfDigitCallback(dtdel);
      onMessageWaitingCallback(mwidel);

      // Initialize pjsip...
      int status = start();
      // set initialized flag
      IsInitialized = (status == 0) ? true : false;

      return status;
    }

    /// <summary>
    /// Shutdown pjsip stack
    /// </summary>
    /// <returns></returns>
    public override int shutdown()
    {
      return dll_shutdown();
    }

    /////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Register all configured accounts 
    /// </summary>
    /// <returns></returns>
    public override int registerAccounts()
    {
      if (!IsInitialized) return -1;

      if (Config.NumOfAccounts <= 0) return 0;

      // unregister accounts
      dll_removeAccounts();

      // iterate all accounts
      for (int i = 0; i < Config.NumOfAccounts; i++)
      {
        IAccount acc = Config.getAccount(i);
        // check if accounts available
        if (null == acc) return -1;

        // reset account state
        BaseAccountStateChanged(i, 0);

        if (acc.Id.Length > 0)
        {
          if (acc.HostName == "0") continue;

          string displayName = acc.DisplayName; 
          // Publish do not work if display name in uri 
          //string uri = displayName + "<sip:" + acc.Id + "@" + acc.HostName + ">";
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
            proxy = "sip:"+acc.ProxyAddress;
          }
          
          dll_registerAccount(uri, reguri, domain, username, password, proxy, (i == Config.DefaultAccountIndex ? true : false));

          // todo:::check if accId corresponds to account index!!!
        }
      }
      return 1;
    }

    /// <summary>
    /// Add new entry in a buddy list and subscribe presence
    /// </summary>
    /// <param name="ident">Buddy address (without hostname part</param>
    /// <param name="presence">subscribe presence flag</param>
    /// <returns></returns>
    public override int addBuddy(string ident, bool presence)
    {
      string uri = "sip:" + ident + "@" + Config.getAccount(Config.DefaultAccountIndex).HostName;
      return dll_addBuddy(uri, presence);
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
    public override int sendMessage(string dest, string message)
    {
      string uri = "sip:" + dest + "@" + Config.getAccount(Config.DefaultAccountIndex).HostName;
      return dll_sendMessage(Config.DefaultAccountIndex, uri, message);
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

    /// <summary>
    /// Get codec by index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public override string getCodec(int index)
    {
      StringBuilder codec = new StringBuilder(256);
      dll_getCodec(index, codec);
      return (codec.ToString());
    }

    /// <summary>
    /// Get number of all codecs
    /// </summary>
    /// <returns></returns>
    public override int getNoOfCodecs()
    {
      if (!IsInitialized) return 0;

      int no = dll_getNumOfCodecs();
      return no;
    }

    /// <summary>
    /// Set codec priority
    /// </summary>
    /// <param name="codecname"></param>
    /// <param name="priority"></param>
    public override void setCodecPriority(string codecname, int priority)
    {
      if (!IsInitialized) return;

      dll_setCodecPriority(codecname, priority);
    }

    /// <summary>
    /// Call proxy factory method
    /// </summary>
    /// <returns></returns>
    public override ICallProxyInterface createCallProxy()
    {
      return new CSipCallProxy(Config);
    }

    #endregion Methods

    #region Callbacks

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callId"></param>
    /// <param name="callState"></param>
    /// <returns></returns>
    private static int onCallStateChanged(int callId, int callState)
    {
      GetInstance().BaseCallStateChanged(callId, callState, "");
      return 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callId"></param>
    /// <param name="sturi"></param>
    /// <returns></returns>
    private static int onCallIncoming(int callId, StringBuilder sturi)
    {
      string uri = sturi.ToString();
      string display  = "";
      string number = "";
    
      // get indices
      int startNum = uri.IndexOf("<sip:");
      int atPos = uri.IndexOf('@');
      // search for number
      if ((startNum >= 0)&&(atPos > startNum))
      {
        number = uri.Substring(startNum + 5, atPos - startNum - 5);  
      }

      // extract display name if exists
      if (startNum >= 0)
      {
        display = uri.Remove(startNum).Trim();
      }
      else 
      { 
        int semiPos = display.IndexOf(';');
        if (semiPos >= 0)
        {
          display = display.Remove(semiPos);
        }
        else 
        {
          int colPos = display.IndexOf(':');
          if (colPos >= 0)
          {
            display = display.Remove(colPos);
          }
        }

      }
      // invoke callback
      GetInstance().BaseIncomingCall(callId, number, display);
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
      GetInstance().Config.getAccount(accId).RegState = regState;
      GetInstance().BaseAccountStateChanged(accId, regState);
      return 1;
    }


    private static int onCallHoldConfirm(int callId)
    {
      //CStateMachine sm = CallManager.getCall(callId);
      //if (sm != null) sm.getState().onHoldConfirm();
      // TODO:::implement proper callback
      GetInstance().BaseCallNotification(callId, ECallNotification.CN_HOLDCONFIRM, "");
      return 1;
    }

    //////////////////////////////////////////////////////////////////////////////////

    private static int onMessageReceived(StringBuilder from, StringBuilder text)
    {
      GetInstance().BaseMessageReceived(from.ToString(), text.ToString());
      return 1;
    }

    private static int onBuddyStatusChanged(int buddyId, int status, StringBuilder text)
    {
      GetInstance().BaseBuddyStatusChanged(buddyId, status, text.ToString());
      return 1;
    }

    private static int onDtmfDigitCallback(int callId, int digit)
    {
      GetInstance().BaseDtmfDigitReceived(callId, digit);
      return 1;
    }

    private static int onMessageWaitingCallback(int mwi, StringBuilder info)
    {
      GetInstance().BaseMessageWaitingIndication(mwi, info.ToString());
      return 1;
    }

    #endregion Callbacks

  }



} // namespace PjsipWrapper
