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


namespace Sipek.Common
{
  /// <summary>
  /// Call control timer types
  /// </summary>
  public enum ETimerType
  {
    ENOREPLY,
    ERELEASED,
    ENORESPONSE,
  }

  /// <summary>
  /// General state machine interface. 
  /// </summary>
  public abstract class IStateMachine
  {
    #region Public Methods
    public abstract void changeState(EStateId stateId);
    public abstract void destroy();
    public abstract bool IsNull { get; }
    #endregion

    #region Public Properties
    public abstract string CallingName { get; set; }
    public abstract string CallingNumber { get; set; }
    public abstract bool Incoming { get; set; }
    public abstract bool Is3Pty { get; set; }
    public abstract bool IsHeld { get; set; }
    public abstract int Session { get; set; }
    public abstract TimeSpan RuntimeDuration { get; }
    public abstract TimeSpan Duration { get; set; }
    public abstract EStateId StateId { get; }
    public abstract string Codec { get; }
    internal abstract bool DisableStateNotifications { get; set; }
    internal abstract int NumberOfCalls { get; }
    #endregion

    #region Internal Methods
    internal abstract bool startTimer(ETimerType ttype);
    internal abstract bool stopTimer(ETimerType ttype);
    internal abstract void stopAllTimers();
    internal abstract void activatePendingAction();
    #endregion

    #region Internal Properties
    internal abstract IAbstractState State { get; }
    internal abstract bool RetrieveRequested { get; set; }
    internal abstract bool HoldRequested { get; set; }
    internal abstract ICallProxyInterface CallProxy { get; }
    internal abstract IConfiguratorInterface Config { get; }
    internal abstract IMediaProxyInterface MediaProxy { get; }
    internal abstract ECallType Type { get; set; }
    internal abstract DateTime Time { get; set; }
    internal abstract bool Counting { get; set; }
    #endregion

  }


  #region Null Pattern
  internal class NullStateMachine : IStateMachine
  {
    public NullStateMachine() : base() { }

    public override EStateId StateId
    {
      get { return EStateId.NULL; }
    }

    public override string CallingName
    {
      get
      {
        return "";
      }
      set
      {
        ;
      }
    }

    public override string CallingNumber
    {
      get
      {
        return "";
      }
      set
      {
        ;
      }
    }

    public override bool Incoming
    {
      get
      {
        return false;
      }
      set
      {
        ;
      }
    }

    public override bool Is3Pty
    {
      get
      {
        return false;
      }
      set
      {
        ;
      }
    }

    public override bool IsHeld
    {
      get
      {
        return false;
      }
      set
      {
        ;
      }
    }

    public override int Session
    {
      get
      {
        return -1;
      }
      set
      {
        ;
      }
    }

    internal override IAbstractState State
    {
      get { return new NullState(); }
    }


    public override void changeState(EStateId stateId)
    {
      ;
    }

    public override void destroy()
    {
      ;
    }

    public override bool IsNull
    {
      get
      {
        return true;
      }
    }

    internal override bool RetrieveRequested
    {
      get
      {
        return false;
      }
      set
      {
        ;
      }
    }

    internal override bool HoldRequested
    {
      get
      {
        return false;
      }
      set
      {
        ;
      }
    }

    internal override ICallProxyInterface CallProxy
    {
      get
      {
        return new NullCallProxy();
      }
    }

    internal override IConfiguratorInterface Config
    {
      get
      {
        return new NullConfigurator();
      }
    }

    internal override IMediaProxyInterface MediaProxy
    {
      get { return new NullMediaProxy(); }
    }

    internal override ECallType Type
    {
      get
      {
        return ECallType.EDialed;
      }
      set
      {
        ;
      }
    }

    internal override bool startTimer(ETimerType ttype)
    {
      return false;
    }

    internal override bool stopTimer(ETimerType ttype)
    {
      return false;
    }

    internal override void stopAllTimers()
    {
      ;
    }

    internal override DateTime Time
    {
      get
      {
        return new DateTime();
      }
      set
      {
        ;
      }
    }

    public override TimeSpan Duration
    {
      get
      {
        return new TimeSpan();
      }
      set
      {
        ;
      }
    }

    public override TimeSpan RuntimeDuration
    {
      get { return new TimeSpan(); }
    }

    internal override void activatePendingAction()
    {
      ;
    }

    internal override bool Counting
    {
      get
      {
        return false;
      }
      set
      {
        ;
      }
    }

    public override string Codec
    {
      get { return "PCMA"; }
    }

    internal override bool DisableStateNotifications
    {
      get
      {
        return true;
      }
      set
      {
        ;
      }
    }

    internal override int NumberOfCalls
    {
      get { return 0; }
    }
  }
  #endregion

}
