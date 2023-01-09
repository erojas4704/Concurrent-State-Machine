using System;
using System.Linq;

namespace csm
{
    [Serializable]
    public abstract class State
    {
        public int priority { get; init; }
        public int group { get; init; } = -1;
        public float time = 0f;

        public delegate void NextStateCallback(Actor actor, Action action);
        public delegate void EnterStateCallback(Type stateType);

        public delegate void ExitStateCallback(Type stateType);
        public delegate void ExitStateCallback<TState>();

        public virtual void Init(Actor actor) { }
        public virtual void Init(Actor actor, Action initiator) { Init(actor); }


        public virtual void Update(Actor actor) { }
        public virtual void Process(Actor actor, Action action) { Next(actor, action); }
        public virtual void End(Actor actor) { }

        public EnterStateCallback Enter;
        public ExitStateCallback Exit;
        public NextStateCallback Next = (a, action) => { };

        public Type[] requiredStates = new Type[] { };
        public Type[] negatedStates = new Type[] { };
        public Type[] partnerStates = new Type[] { };

        public bool solo;

        public State()
        {
            StateDescriptor desc = (StateDescriptor)System.Attribute.GetCustomAttribute(this.GetType(), typeof(StateDescriptor));
            if (desc != null)
            {
                priority = desc.priority;
                group = desc.group;
            }

            Negate neg = (Negate)System.Attribute.GetCustomAttribute(this.GetType(), typeof(Negate));
            if (neg != null) negatedStates = neg.states;

            Require req = (Require)System.Attribute.GetCustomAttribute(this.GetType(), typeof(Require));
            if (req != null) requiredStates = req.states;

            With with = (With)System.Attribute.GetCustomAttribute(this.GetType(), typeof(With));
            if (with != null) partnerStates = with.states;

            Solo solo = (Solo)System.Attribute.GetCustomAttribute(this.GetType(), typeof(Solo));
            if (solo != null) this.solo = solo.solo;
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

//! hack
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}