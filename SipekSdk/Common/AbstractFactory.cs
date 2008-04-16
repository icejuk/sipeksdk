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
 * @see http://voipengine.googlepages.com/
 */

using System;
using System.Collections.Generic;
using System.Text;

using Sipek.Common.CallControl;


/*! \namespace Common
    \brief Common namespace defines general interfaces to various VoIP functionality

    ...
*/

namespace Sipek.Common
{

  /// <summary>
  /// AbstractFactory is an abstract interace providing interfaces for CallControl module. 
  /// It consists of two parts: factory methods and getter methods. First creates instances, 
  /// later returns instances. 
  /// </summary>
  public interface AbstractFactory
  {
    // factory methods

    /// <summary>
    /// Factory creator. Creates new instance of timer 
    /// </summary>
    /// <returns>ITimer instance</returns>
    ITimer createTimer();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IStateMachine createStateMachine(CCallManager mng);

    /// <summary>
    /// Factory getter. Returns IMediaProxyInterface reference
    /// </summary>
    /// <returns>IMediaProxyInterface reference</returns>
    IMediaProxyInterface MediaProxy
    {
      get;
      set;
    }

    /// <summary>
    /// Factory getter. Returns reference to call log instance.
    /// </summary>
    /// <returns>ICallLogInterface instance</returns>
    ICallLogInterface CallLogger
    {
      get;
      set;
    }

    /// <summary>
    /// Factory getter. Returns reference to Configurator instance.
    /// </summary>
    /// <returns>IConfiguratorInterface reference</returns>
    IConfiguratorInterface Configurator
    {
      get;
      set;
    }

    /// <summary>
    /// 
    /// </summary>
    IVoipProxy CommonProxy
    {
      get;
      set;
    }
  }

  #region Null Pattern
  /// <summary>
  /// Null Factory implementation
  /// </summary>
  public class NullFactory : AbstractFactory
  {
    IConfiguratorInterface _config = new NullConfigurator();
    IVoipProxy _common = new NullVoipProxy();
    IMediaProxyInterface _media = new NullMediaProxy();
    ICallLogInterface _logger = new NullCallLogger();

    #region AbstractFactory members
    // factory methods
    public ITimer createTimer()
    {
      return new NullTimer();
    }
    public IStateMachine createStateMachine(CCallManager mng)
    {
      return new CStateMachine(mng);
    }

    public IVoipProxy CommonProxy
    {
      get { return _common; }
      set { _common = value;  }
    }

    public IConfiguratorInterface Configurator
    {
      get { return _config; }
      set { _config = value; }
    }

    // Implement getters
    public IMediaProxyInterface MediaProxy
    {
      get { return _media; }
      set { _media = value; }
    }

    public ICallLogInterface CallLogger
    {
      get { return _logger; }
      set { _logger = value; }
    }

    #endregion
  }

  #endregion
}
