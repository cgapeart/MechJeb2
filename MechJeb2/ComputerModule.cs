﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace MuMech
{
    public class ComputerModule : IComparable<ComputerModule>
    {
        public Part part = null;
        public MechJebCore core = null;
        public VesselState vesselState = null;

        //conveniences:
        public Vessel vessel { get { return part.vessel; } }
        public CelestialBody mainBody { get { return part.vessel.mainBody; } }
        public Orbit orbit { get { return part.vessel.orbit; } }

        public int priority = 0;

        public int CompareTo(ComputerModule other)
        {
            if (other == null) return 1;
            return priority.CompareTo(other.priority);
        }

        protected bool _enabled = false;
        public bool enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    if (_enabled)
                    {
                        OnModuleEnabled();
                    }
                    else
                    {
                        OnModuleDisabled();
                    }
                }
            }
        }

        //The UserPool is an alternative way to handle enabling/disabling of a ComputerModule. 
        //Users can add and remove themselves from the user pool and the ComputerModule will be
        //enabled if and only if there is at least one user. For consistency, it's probably
        //best that a given ComputerModule be controlled either entirely through enabled, or
        //entirely through users, and that the two not be mixed.
        public UserPool users;

        public ComputerModule(MechJebCore core)
        {
            this.core = core;
            part = core.part;
            vesselState = core.vesselState;

            users = new UserPool(this);
        }

        public virtual void OnModuleEnabled()
        {
        }

        public virtual void OnModuleDisabled()
        {
        }

        public virtual void OnControlLost()
        {
        }

        public virtual void Drive(FlightCtrlState s)
        {
        }

        public virtual void OnStart(PartModule.StartState state)
        {
        }

        public virtual void OnActive()
        {
        }

        public virtual void OnInactive()
        {
        }

        public virtual void OnAwake()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnLoad(ConfigNode local, ConfigNode type, ConfigNode global)
        {
            try
            {
                if (global != null) ConfigNode.LoadObjectFromConfig(this, global, (int)Pass.Global);
                if (type != null) ConfigNode.LoadObjectFromConfig(this, type, (int)Pass.Type);
                if (local != null) ConfigNode.LoadObjectFromConfig(this, local, (int)Pass.Local);
            }
            catch (Exception e)
            {
                Debug.Log("MechJeb caught exception in OnLoad for " + this.GetType().Name + ": " + e);
            }
        }

        public virtual void OnSave(ConfigNode local, ConfigNode type, ConfigNode global)
        {
            try
            {
                if (global != null) ConfigNode.CreateConfigFromObject(this, (int)Pass.Global).CopyTo(global);
                if (type != null) ConfigNode.CreateConfigFromObject(this, (int)Pass.Type).CopyTo(type);
                if (local != null) ConfigNode.CreateConfigFromObject(this, (int)Pass.Local).CopyTo(local);
            }
            catch (Exception e)
            {
                Debug.Log("MechJeb caught exception in OnSave for " + this.GetType().Name + ": " + e);
            }
        }

        public virtual void OnDestroy()
        {
        }

        public void print(object message)
        {
            MonoBehaviour.print(message);
        }
    }

    [Flags]
    public enum Pass
    {
        Local = 1,
        Type = 2,
        Global = 4
    }

    //Lets multiple users enable and disable a computer module, such that the 
    //module only gets disabled when all of its users have disabled it.
    public class UserPool : List<object>
    {
        ComputerModule controlledModule;

        public UserPool(ComputerModule controlledModule)
            : base()
        {
            this.controlledModule = controlledModule;
        }

        public new void Add(object user)
        {
            if (!base.Contains(user))
            {
                base.Add(user);
            }
            controlledModule.enabled = true;
        }

        public new void Remove(object user)
        {
            if (base.Contains(user))
            {
                base.Remove(user);
            }
            if (base.Count == 0) controlledModule.enabled = false;
        }

        public new void Clear()
        {
            base.Clear();
            controlledModule.enabled = false;
        }

        public bool RecursiveUser(object user)
        {
            if (base.Contains(user))
            {
                return true;
            }
            else
            {
                return this.OfType<ComputerModule>().ToList().Exists(c => (c != controlledModule) && c.users.RecursiveUser(user));
            }
        }
    }
}
