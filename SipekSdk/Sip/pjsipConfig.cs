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
using System.Runtime.InteropServices;

namespace Sipek.Sip
{
  #region Config Structure

  [StructLayout(LayoutKind.Sequential)]
  public class SipConfigStruct
  {
    private static SipConfigStruct _instance = null;
    public static SipConfigStruct Instance
    {
      get 
      {
        if (_instance == null) _instance = new SipConfigStruct();
        return _instance;
      }
    }

    public int listenPort = 5060;
    [MarshalAs(UnmanagedType.I1)]   // warning:::Marshal managed bool type to unmanaged (C) bool !!!!
    public bool useTLS = false;
    [MarshalAs(UnmanagedType.I1)]
    public bool noUDP = false;
    [MarshalAs(UnmanagedType.I1)]
    public bool noTCP = true;
    [MarshalAs(UnmanagedType.ByValTStr,SizeConst=255)]
    public string stunServer;
    [MarshalAs(UnmanagedType.I1)]
    public bool publishEnabled = false;
    // IMS specifics
    [MarshalAs(UnmanagedType.I1)]
    public bool imsEnabled = false;
    [MarshalAs(UnmanagedType.I1)]
    public bool secAgreement = false; // rfc 3329
    [MarshalAs(UnmanagedType.I1)]
    public bool ipsecHeaders = false; 
    [MarshalAs(UnmanagedType.I1)]
    public bool useIPSecTransport = false; 
  }

  #endregion
}
