using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace CSM
{
    [Serializable]
    public abstract class State
    {
        public Actor actor;
        public bool solo;
        public int Priority { get; init; }
        public int Group { get; init; } = -1;

        public float time; //TODO z-61. Keep a reference to start time.
        public float expiresAt;

        public Stats stats;

        public delegate void ExitStateHandler(State state);

        public virtual void Init(Message initiator)
        {
        }

        /** Processes an update cycle, this method is called once every frame.
         *  Return: Can return a new set of stats for the actor, or null if no stat changes necessary.
         */
        public virtual void Update()
        {
        }

        public virtual bool Process(Message message) => false;

        public virtual void End()
        {
        }

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

        public virtual void SetStats(Stats actorStats) => stats = actorStats;
    }

    public abstract class State<TStatType> : State where TStatType : Stats
    {
        public new TStatType stats;

        public override void SetStats(Stats actorStats)
        {
            if (actorStats is TStatType castStats)
            {
                stats = castStats;
            }
            else
            {
                throw new InvalidOperationException("Invalid stats type for this state");
            }
        }
    }
}

//! hack
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}