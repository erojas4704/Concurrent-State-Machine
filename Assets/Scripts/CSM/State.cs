using System;
using System.Linq;

namespace CSM
{
    [Serializable]
    public abstract class State
    {
        public bool solo;
        public int Priority { get; init; }
        public int Group { get; init; } = -1;
        public float time;

        public delegate void ExitStateHandler(State state);

        public virtual void Init(Actor actor, Message initiator) {}

        /** Processes an update cycle, this method is called once every frame.
         *  Return: Can return a new set of stats for the actor, or null if no stat changes necessary.
         */
        public virtual Stats Update(Actor actor, Stats stats) => null;
        public virtual bool Process(Actor actor, Message message) => false;
        public virtual void End(Actor actor) { }

        // ReSharper disable once InconsistentNaming
        public ExitStateHandler OnExit;

        protected void Exit() => OnExit?.Invoke(this);

        public Type[] requiredStates = { };
        public Type[] negatedStates = { };
        public Type[] partnerStates = { };
        
        protected State()
        {
            StateDescriptor desc = (StateDescriptor)Attribute.GetCustomAttribute(GetType(), typeof(StateDescriptor));
            if (desc != null)
            {
                Priority = desc.priority;
                Group = desc.group;
            }

            Negate neg = (Negate)Attribute.GetCustomAttribute(GetType(), typeof(Negate));
            if (neg != null) negatedStates = neg.states;

            Require req = (Require)Attribute.GetCustomAttribute(GetType(), typeof(Require));
            if (req != null) requiredStates = req.states;

            With with = (With)Attribute.GetCustomAttribute(GetType(), typeof(With));
            if (with != null) partnerStates = with.states;

            Solo attrSolo = (Solo)Attribute.GetCustomAttribute(GetType(), typeof(Solo));
            if (attrSolo != null) solo = attrSolo.solo;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            return obj.GetType() == GetType();
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override string ToString()
        {
            return GetType().ToString().Split('.').Last();
        }
    }
}

//! hack
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}