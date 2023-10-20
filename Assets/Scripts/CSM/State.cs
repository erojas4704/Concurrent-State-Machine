using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CSM
{
    [Serializable]
    public abstract class State
    {
        public Actor actor;
        public bool solo;
        public int Priority { get; init; }
        public int Group { get; init; } = -1;

        public float startTime;
        /**Time for ghost state to expire.*/
        public float expiresAt;

        public Stats stats;

        public bool IsGhost => expiresAt > Time.time;

        public delegate void ExitStateHandler(State state, float persistDuration, params string[] messagesToListenFor);

        public virtual void Init(Message initiator) { }

        /** Processes an update cycle, this method is called once every frame.
         *  Return: Can return a new set of stats for the actor, or null if no stat changes necessary.
         */
        public virtual void Update() { }

        public virtual bool Process(Message message) => false;

        public virtual void End() { }

        // ReSharper disable once InconsistentNaming
        public ExitStateHandler OnExit;

        protected void Exit(float persistDuration = 0f, params string[] messagesToListenFor) =>
            OnExit?.Invoke(this, persistDuration, messagesToListenFor);

        public HashSet<Type> requiredStates = new HashSet<Type>();
        public HashSet<Type> negatedStates = new HashSet<Type>();
        public HashSet<Type> partnerStates = new HashSet<Type>();

        protected State()
        {
            StateDescriptor desc = (StateDescriptor)Attribute.GetCustomAttribute(GetType(), typeof(StateDescriptor));
            if (desc != null)
            {
                Priority = desc.priority;
                Group = desc.group;
            }

            Negate neg = (Negate)Attribute.GetCustomAttribute(GetType(), typeof(Negate));
            if (neg != null) negatedStates = new HashSet<Type>(neg.states);

            Require req = (Require)Attribute.GetCustomAttribute(GetType(), typeof(Require));
            if (req != null) requiredStates = new HashSet<Type>(req.states);

            With with = (With)Attribute.GetCustomAttribute(GetType(), typeof(With));
            if (with != null) partnerStates = new HashSet<Type>(with.states);

            Solo attrSolo = (Solo)Attribute.GetCustomAttribute(GetType(), typeof(Solo));
            if (attrSolo != null) solo = attrSolo.solo;
        }

        public void ValidateRequirements()
        {
            if (solo)
            {
                if (partnerStates.Count > 0 || requiredStates.Count > 0)
                {
                    throw new CsmException(
                        $"State {this} is a solo state, but requires {partnerStates.Count + requiredStates.Count} other states.",
                        GetType());
                }
            }

            if (partnerStates.Contains(GetType()) || requiredStates.Contains(GetType()))
            {
                throw new CsmException($"State {this} requires itself. This behavior is not supported.",
                    GetType());
            }

            if (negatedStates.Contains(GetType()))
            {
                throw new CsmException($"State {this} negates itself.", GetType());
            }

            if (Group >= 0)
            {
                CheckGroupsInRequirements(partnerStates);
                CheckGroupsInRequirements(requiredStates);
            }
        }

        //TODO Z-67: do not run these checks in production builds
        private void CheckGroupsInRequirements(HashSet<Type> requirements)
        {
            foreach (Type requiredStateType in requirements)
            {
                object[] attributes = requiredStateType.GetCustomAttributes(true);
                if (attributes.OfType<StateDescriptor>().Any(descriptor => descriptor.group == Group))
                {
                    throw new CsmException(
                        $"State {this} requires state {requiredStateType}, but they are in the same grouping",
                        GetType());
                }
            }
        }

        public float Timer => Time.time - startTime;

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
                throw new CsmException("Invalid stats type for this state.", GetType());
            }
        }
    }
}

//! hack
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}