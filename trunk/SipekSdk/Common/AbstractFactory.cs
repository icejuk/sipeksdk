using System;
using System.Collections.Generic;
using System.Text;



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
    /// Factory creator. Creates new instance of call proxy. 
    /// </summary>
    /// <returns>ICallProxyInterface instance</returns>
    ICallProxyInterface createCallProxy();

    /// <summary>
    /// Factory getter. Returns IMediaProxyInterface reference
    /// </summary>
    /// <returns>IMediaProxyInterface reference</returns>
    IMediaProxyInterface getMediaProxy();

    /// <summary>
    /// Factory getter. Returns reference to call log instance.
    /// </summary>
    /// <returns>ICallLogInterface instance</returns>
    ICallLogInterface getCallLogger();

    /// <summary>
    /// Factory getter. Returns reference to Configurator instance.
    /// </summary>
    /// <returns>IConfiguratorInterface reference</returns>
    IConfiguratorInterface getConfigurator();

    /// <summary>
    /// Factory getter. Returns an instance of VoIP proxy interface
    /// </summary>
    /// <returns>IVoipProxy instance</returns>
    IVoipProxy getCommonProxy();
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
    ICallLogInterface _logger = new CNullCallLog();

    #region AbstractFactory members
    // factory methods
    public ITimer createTimer()
    {
      return new NullTimer();
    }

    //TODO
    public ICallProxyInterface createCallProxy()
    {
      return new NullCallProxy();
    }

    public IVoipProxy getCommonProxy()
    {
      return _common;
    }

    public IConfiguratorInterface getConfigurator()
    {
      return _config;
    }

    // Implement getters
    public IMediaProxyInterface getMediaProxy()
    {
      return _media;
    }

    public ICallLogInterface getCallLogger()
    {
      return _logger;
    }
    #endregion
  }

  #endregion
}
