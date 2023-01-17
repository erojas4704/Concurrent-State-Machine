using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace CSM
{
    public class Actor : MonoBehaviour, ISerializationCallbackReceiver
    {
        protected StateSet states = new StateSet(new StateComparer());
        private Queue<State> slatedForDeletion = new Queue<State>();
        private Queue<StateAndInitiator> slatedForCreation = new Queue<StateAndInitiator>();
        private Queue<Action> actionBuffer = new Queue<Action>();
        protected HashSet<State> statePool = new HashSet<State>();
        
        public Vector3 velocity;
        public Stats stats;
        public Stats finalStats;

        public delegate void StateChangeHandler(Actor actor);
        public event StateChangeHandler OnStateChange;

        public float actionTimer = .75f;

        public virtual void Update()
        {
            foreach (State state in states)
            {
                state.time += Time.deltaTime;
                state.Update(this);
            }
            processQueues();
            ProcessActionBuffer();
        }

        public bool Is<TState>()
        {
            return states.Any(s => s.GetType() == typeof(TState));
        }
        
        public void OnStateChangeHandler(Actor actor)
        {
            CalculateStats();
        }

        public void EnterState<T>() where T : State
        {
            EnterState(typeof(T));
        }

        public void EnterState<T>(Action initiator) where T : State
        {
            EnterState(typeof(T), initiator);
        }

        public void EnterState(Type stateType)
        {
            EnterState(stateType, null);
        }

        public void EnterState(Type stateType, Action initiator = null)
        {
            State pooledState = statePool.FirstOrDefault<State>(s => s.GetType() == stateType);
            State newState = pooledState != null ? pooledState : (State)Activator.CreateInstance(stateType);
            if (newState.group > -1) ExitStateGroup(newState.group);

            StateAndInitiator si = new StateAndInitiator(
               newState, initiator
            );

            slatedForCreation.Enqueue(si);
        }
        
        //TODO allow to insert this middleware in and consolidate loops for performance reasons.
        public void CalculateStats()
        {
            Stats lastCalculatedStat = this.stats;
            foreach (State state in states)
            {
                    lastCalculatedStat = state.Reduce(this, lastCalculatedStat);
                    state.stats = lastCalculatedStat;
            }

            finalStats = lastCalculatedStat;
        }

        private void ExitState(State state)
        {
            slatedForDeletion.Enqueue(state);
        }

        private void ExitState(Type stateType)
        {
            //TODO o(n) implement hashset
            State state = null;
            foreach (State s in states)
            {
                if (s.GetType() == stateType)
                {
                    state = s;
                    break;
                }
            }

            if (state != null) ExitState(state);
        }

        private void processQueues()
        {
            bool changed = false;
            while (slatedForDeletion.Count > 0)
            {
                State state = slatedForDeletion.Dequeue();
                states.Remove(state);
                statePool.Add(state);
                state.End(this);
                changed = true;
            }

            while (slatedForCreation.Count > 0)
            {
                //TODO extract methods here.
                StateAndInitiator si = slatedForCreation.Dequeue();
                State newState = si.state;
                if (!HasRequirements(newState)) continue;
                foreach (Type negatedState in newState.negatedStates) ExitState(negatedState);
                foreach (Type partnerState in newState.partnerStates) EnterState(partnerState);
                if (newState.solo) ExitAllStatesExcept(newState);

                states.Add(newState);
                statePool.Remove(newState);
                newState.Enter = EnterState;
                newState.Exit = ExitState;
                if (si.initiator != null)
                    newState.Init(this, si.initiator);
                else
                    newState.Init(this);
                newState.time = 0;
                changed = true;
            }

            UpdateStateChain();

            if (changed && OnStateChange != null)
                OnStateChange(this);
        }

        private void ExitAllStatesExcept(State state)
        {
            foreach (State s in states) if (s != state) ExitState(s);
        }

        private bool HasRequirements(State state)
        {
            //TODO o(n^2) implement hashset
            foreach (Type requiredState in state.requiredStates)
            {
                if (!states.Any(s => s.GetType() == requiredState))
                {
                    Debug.LogWarning($"Not entering state {state} because dependency {requiredState} is missing!");
                    return false;
                }
            }
            return true;
        }

        private void ProcessActionBuffer()
        {
            if (actionBuffer.Count < 1) return;

            Action firstAction = actionBuffer.Peek();
            firstAction.timer += Time.deltaTime;

            if (PropagateAction(firstAction, false))
            {
                actionBuffer.Dequeue();
            }
            else if (firstAction.timer >= actionTimer)
            {
                actionBuffer.Dequeue();
            }
        }

        public bool PropagateAction(Action action, bool buffer = true)
        {
            if (states.Count < 1)
            {
                throw new Exception("This Actor has no states!");
            }
            states.Min.Process(this, action);
            if (ShouldBufferAction(action, buffer))
                actionBuffer.Enqueue(action);

            return action.processed;
        }

        private bool ShouldBufferAction(Action action, bool buffer)
        {
            return !action.processed && buffer && action.phase == Action.ActionPhase.Pressed;
        }

        private void ExitStateGroup(int group)
        {
            foreach (State state in states)
                if (state.group == group)
                    ExitState(state.GetType());
        }

        private void UpdateStateChain()
        {
            State prev = null;
            foreach (State s in states)
            {
                s.Next = (Actor actor, Action action) => {};
                if (prev != null)
                {
                    prev.Next = s.Process;
                }

                prev = s;
            }
        }
        
        public T GetState<T>() where T:State
        {
            foreach (State s in states)
            {
                if (s.GetType() == typeof(T))
                {
                    return (T)s;
                }
            }

            return null;
        }

        public StateSet GetStates()
        {
            return states;
        }

        private class StateAndInitiator
        {
            public readonly State state;
            public readonly Action initiator;

            public StateAndInitiator(State state, Action initiator)
            {
                this.state = state;
                this.initiator = initiator;
            }
        }

        #region ISerializationCallbackReceiver implementation
        [SerializeReference, HideInInspector]
        private State[] _stateArray;
        public void OnBeforeSerialize()
        {
            _stateArray = new State[states.Count];
            states.CopyTo(_stateArray);
        }
        public void OnAfterDeserialize()
        {
            if (_stateArray == null) return;
            states = new StateSet();
            foreach (State s in _stateArray) EnterState(s.GetType());
            OnStateChange += OnStateChangeHandler;
        }
        #endregion
    }
}