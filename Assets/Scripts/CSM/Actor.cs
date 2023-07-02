using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CSM
{
    public class Actor : MonoBehaviour, ISerializationCallbackReceiver
    {
        private StateStack statesStack = new();

        /**States scheduled to be deleted this frame. This work is done at the end of an update cycle.*/
        private readonly Queue<State> slatedForDeletion = new();

        /**States scheduled to be initialized this frame. This work is done at the end of an update cycle. */
        private readonly Queue<StateAndInitiator> slatedForCreation = new();

        /** Buffered messages for player input buffering. */
        private readonly Queue<Action> actionBuffer = new();

        /** Pool of states that have been removed. This prevents GC running on expired states. */
        private readonly Dictionary<Type, State> statePool = new();

        public Vector3 velocity;
        public Stats stats;
        [SerializeField] private Stats finalStats;

        public delegate void StateChangeHandler(Actor actor);

        public event StateChangeHandler OnStateChange;

        public float actionTimer = .75f;

        public virtual void Update()
        {
            foreach (State state in statesStack)
            {
                state.time += Time.deltaTime;
                state.Update(this);
            }

            ProcessQueues();
            ProcessActionBuffer();
        }

        public bool Is<TState>() => statesStack.Contains(typeof(TState));

        private void OnStateChangeHandler(Actor actor) => CalculateStats();

        public void EnterState<T>() where T : State
        {
            EnterState(typeof(T));
        }

        public void EnterState<T>(Action initiator) where T : State
        {
            EnterState(typeof(T), initiator);
        }

        private void EnterState(Type stateType)
        {
            EnterState(stateType, null);
        }

        private void EnterState(Type stateType, Action initiator)
        {
            statePool.TryGetValue(stateType, out State pooledState);
            State newState = pooledState ?? (State)Activator.CreateInstance(stateType);
            if (newState.Group > -1) ExitStateGroup(newState.Group);

            StateAndInitiator si = new(
                newState, initiator
            );

            slatedForCreation.Enqueue(si);
        }

        //TODO allow to insert this middleware in and consolidate loops for performance reasons. 
        // What the FUCK does that mean ^
        private void CalculateStats()
        {
            Stats lastCalculatedStat = stats;
            foreach (State state in statesStack)
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
            State state = null;
            foreach (State s in statesStack)
            {
                if (s.GetType() == stateType)
                {
                    state = s;
                    break;
                }
            }

            if (state != null) ExitState(state);
        }

        /**Creates and deletes all states that are slated for creation or deletion. This method handles state pooling and
         * resolving dependencies.
         */
        private void ProcessQueues()
        {
            bool changed = false;
            while (slatedForDeletion.Count > 0)
            {
                State state = slatedForDeletion.Dequeue();
                statesStack.Remove(state);
                statePool.Add(state.GetType(), state);
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

                statesStack.Add(newState);
                statePool.Remove(newState.GetType());
                newState.Enter = EnterState;
                newState.Exit = ExitState;
                if (si.initiator != null)
                    newState.Init(this, si.initiator); //Might be redundant. Consider removing conditional
                else
                    newState.Init(this);
                newState.time = 0;
                changed = true;
            }

            if (changed && OnStateChange != null)
                OnStateChange(this);
        }

        private void ExitAllStatesExcept(State state)
        {
            foreach (State s in statesStack)
                if (!Equals(s, state))
                    ExitState(s);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private bool HasRequirements(State state)
        {
            foreach (Type requiredState in state.requiredStates)
            {
                if (statesStack.Contains(requiredState)) continue;
                Debug.LogWarning($"Not entering state {state} because dependency {requiredState} is missing!");
                return false;
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

        protected bool PropagateAction(Action action, bool buffer = true)
        {
            if (statesStack.Count < 1)
            {
                throw new Exception("This Actor has no states!");
            }

            foreach (State s in statesStack)
            {
                if(s.Process(this, action)) break;
                if (action.cancelled) break;
            }

            if (ShouldBufferAction(action, buffer))
                actionBuffer.Enqueue(action);

            return action.processed;
        }

        private static bool ShouldBufferAction(Action action, bool buffer) =>
            !action.processed && buffer && action.phase == Action.ActionPhase.Pressed;

        private void ExitStateGroup(int group)
        {
            foreach (State state in statesStack)
                if (state.Group == group)
                    ExitState(state.GetType());
        }

        public StateStack GetStates()
        {
            return statesStack;
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

        [SerializeReference, HideInInspector] private State[] stateArray;

        public void OnBeforeSerialize()
        {
            stateArray = new State[statesStack.Count];
            statesStack.CopyTo(stateArray);
        }

        public void OnAfterDeserialize()
        {
            if (stateArray == null) return;
            statesStack = new StateStack();
            foreach (State s in stateArray) EnterState(s.GetType());
            OnStateChange += OnStateChangeHandler;
        }

        #endregion
    }
}