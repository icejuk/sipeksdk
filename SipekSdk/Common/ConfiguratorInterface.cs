using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{
  /// <summary>
  /// IConfiguratorInterface defines data access interface.
  /// </summary>
  public interface IConfiguratorInterface
  {
    /// <summary>
    /// Do Not Disturb Property
    /// </summary>
    bool DNDFlag { get; set; }
    /// <summary>
    /// Auto Answer property
    /// </summary>
    bool AAFlag { get; set; }
    /// <summary>
    /// Call Forwarding Unconditional property
    /// </summary>
    bool CFUFlag { get; set; }
    /// <summary>
    /// Call Forwarding Unconditional Number property
    /// </summary>
    string CFUNumber { get; set; }
    /// <summary>
    /// Call Forwarding No Reply property
    /// </summary>
    bool CFNRFlag { get; set; }
    /// <summary>
    /// Call Forwarding No Reply Number property
    /// </summary>
    string CFNRNumber { get; set; }
    /// <summary>
    /// Call Forwarding Busy property
    /// </summary>
    bool CFBFlag { get; set; }
    /// <summary>
    /// Call Forwarding Busy Number property
    /// </summary>
    string CFBNumber { get; set; }
    /// <summary>
    /// Sip listening port property
    /// </summary>
    int SIPPort { get; set; }
    /// <summary>
    /// Default account index property
    /// </summary>
    int DefaultAccountIndex { get; set; }
    /// <summary>
    /// Number of accounts property
    /// </summary>
    int NumOfAccounts { get; set; }
    /// <summary>
    /// List of all codecs
    /// </summary>
    List<string> CodecList { get; set; }


    #region Public Methods
    /// <summary>
    /// Account getter
    /// </summary>
    /// <param name="index">index for account</param>
    /// <returns>IAccount instance</returns>
    IAccount getAccount(int index);

    /// <summary>
    /// Save settings 
    /// </summary>
    void Save();
    #endregion Methods
  }


  /// <summary>
  /// IAccount interface
  /// </summary>
  public interface IAccount
  {
    /// <summary>
    /// Account name
    /// </summary>
    string AccountName { get; set;}
    /// <summary>
    /// Account host name
    /// </summary>
    string HostName { get; set;}
    /// <summary>
    /// Account Id = Username
    /// </summary>
    string Id { get; set;}
    /// <summary>
    /// Account username
    /// </summary>
    string UserName { get; set;}
    /// <summary>
    /// Account password
    /// </summary>
    string Password { get; set;}
    /// <summary>
    /// Account display
    /// </summary>
    string DisplayName { get; set;}
    /// <summary>
    /// Account Domain name
    /// </summary>
    string DomainName { get; set;}
    /// <summary>
    /// Account registrar port
    /// </summary>
    int Port { get; set;}
    /// <summary>
    /// Account current state (temporary data)
    /// </summary>
    int RegState { get; set;}
    /// <summary>
    /// Account IMS features enable flag
    /// </summary>
    bool ImsEnabled { get; set;}

  }

  #region Null Pattern
  /// <summary>
  /// 
  /// </summary>
  public class NullConfigurator : IConfiguratorInterface
  {
    public class NullAccount : IAccount
    {
      public string AccountName
      {
        get { return ""; }
        set { }
      }

      public string HostName
      {
        get { return ""; }
        set { }
      }

      public string Id
      {
        get { return ""; }
        set { }
      }

      public string UserName
      {
        get { return ""; }
        set { }
      }

      public string Password
      {
        get { return ""; }
        set { }
      }

      public string DisplayName
      {
        get { return ""; }
        set { }
      }

      public string DomainName
      {
        get { return ""; }
        set { }
      }

      public int Port
      {
        get { return 0; }
        set { }
      }

      public int RegState
      {
        get { return 0; }
        set { }
      }

      public bool ImsEnabled
      {
        get { return false; }
        set { }
      }
    }

    #region IConfiguratorInterface Members

    public bool CFUFlag
    {
      get { return false; }
      set { }
    }

    public string CFUNumber
    {
      get { return ""; }
      set { }
    }

    public bool CFNRFlag
    {
      get { return false; }
      set { }
    }

    public string CFNRNumber
    {
      get { return ""; }
      set { }
    }

    public bool CFBFlag
    {
      get { return false; }
      set { }
    }

    public string CFBNumber
    {
      get { return ""; }
      set { }
    }

    public bool DNDFlag
    {
      get { return false; }
      set { }
    }

    public bool AAFlag
    {
      get { return false; }
      set { }
    }
    public int SIPPort
    {
      get { return 5060; }
      set { }
    }
    public int DefaultAccountIndex
    {
      get { return 0; }
      set { }
    }

    public int NumOfAccounts
    {
      get { return 1; }
      set { }
    }

    public IAccount getAccount(int index)
    {
      return new NullAccount();
    }

    public void Save()
    { }

    public List<string> CodecList { get { return null; } set { } }

    #endregion


    #region IConfiguratorInterface Members


    public IAccount getAccount()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    #endregion
  }
  #endregion

}
