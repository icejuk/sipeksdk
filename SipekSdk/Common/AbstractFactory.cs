using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{


  /// <summary>
  /// AbstractFactory is an abstract interace providing interfaces for CallControl module
  /// It consists of two parts: factory methods and getter methods. First creating instances, 
  /// later returns instances. 
  /// </summary>
  public interface AbstractFactory
  {
    // factory methods
    ITimer createTimer();

    ICallProxyInterface createCallProxy();

    // getters
    IMediaProxyInterface getMediaProxy();

    ICallLogInterface getCallLogger();

    IConfiguratorInterface getConfigurator();

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
