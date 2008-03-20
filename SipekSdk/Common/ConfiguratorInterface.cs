using System;
using System.Collections.Generic;
using System.Text;

namespace Sipek.Common
{
  /// <summary>
  /// IConfiguratorInterface
  /// </summary>
  public interface IConfiguratorInterface
  {
    bool DNDFlag { get; set; }
    bool AAFlag { get; set; }
    bool CFUFlag { get; set; }
    string CFUNumber { get; set; }
    bool CFNRFlag { get; set; }
    string CFNRNumber { get; set; }
    bool CFBFlag { get; set; }
    string CFBNumber { get; set; }
    int SIPPort { get; set; }
    int DefaultAccountIndex { get; set; }
    int NumOfAccounts { get; set; }
    List<string> CodecList { get; set; }

    IAccount getAccount();
    IAccount getAccount(int index);

    #region Methods
    void Save();
    #endregion Methods
  }


  /// <summary>
  /// IAccount interface
  /// </summary>
  public interface IAccount
  {
    string AccountName { get; set;}
    string HostName { get; set;}
    string Id { get; set;}
    string UserName { get; set;}
    string Password { get; set;}
    string DisplayName { get; set;}
    string DomainName { get; set;}
    int Port { get; set;}
    int RegState { get; set;}

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
