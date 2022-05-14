using UnityEngine;
using System;
using System.Linq;
using CSM.States;
using System.Runtime.CompilerServices;

namespace CSM
{

    [Serializable]
    public abstract class State
    {
        public int priority;// {get; init;}
        public int group;// {get; init;}
        public float time = 0f;

        public delegate void NextStateCallback(Actor actor, Action action);
        public delegate void EnterStateCallback(Type stateType);

        public delegate void ExitStateCallback(Type stateType);
        public delegate void ExitStateCallback<TState>();

        public virtual void Init(Actor actor) { }
        public virtual void Update(Actor actor) { }
        public virtual void Process(Actor actor, Action action) { }
        public virtual void End(Actor actor) { }

        public EnterStateCallback Enter;
        public ExitStateCallback Exit;
        public NextStateCallback Next = (a, action) => { };

        public State() { 

            
        }

        /// <summary>
        /// Must be called by the state machine to prepare a state. 
        /// This method is not to be overridden.
        /// </summary>
        internal void Prepare()
        {
            // StateDescriptor desc = (StateDescriptor)System.Attribute.GetCustomAttribute(this.GetType(), typeof(StateDescriptor));
            // if (desc != null)
            // {
            //     priority = desc.priority;
            //     group = desc.group;
            // }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            return obj.GetType() == this.GetType();
        }

        public override int GetHashCode()
        {//
            return this.GetType().GetHashCode();
        }

        public override string ToString()
        {
            return this.GetType().ToString().Split('.').Last();
        }
    }
}