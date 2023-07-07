using System.Collections.Generic;
using System;
using Unity.Collections;
using UnityEngine;

namespace CSM
{
    [RequireComponent(typeof(Stats))]
    public class Actor : MonoBehaviour, ISerializationCallbackReceiver
    {
        private StateStack statesStack = new();

        /**States scheduled to be deleted this frame. This work is done at the end of an update cycle.*/
        private readonly Queue<State> slatedForDeletion = new();

        /**States scheduled to be initialized this frame. This work is done at the end of an update cycle. */
        private readonly Queue<StateAndInitiator> slatedForCreation = new();

        /** Buffered messages for player input buffering. */
        private readonly Queue<Message> actionBuffer = new();

        /** Pool of states that have been removed. This prevents GC running on expired states. */
        private readonly Dictionary<Type, State> statePool = new();

        public Vector3 velocity;
        private Stats stats;

        public delegate void StateChangeHandler(Actor actor);

        public event StateChangeHandler OnStateChange;

        public float actionTimer = .75f;

        private void Awake()
        {
            stats = GetComponent<Stats>();
        }

        public virtual void Update()
        {
            stats.Reset();
            foreach (State state in statesStack)
            {
                state.time += Time.deltaTime;
                //TODO <- Consider performance implications of unnecessary record cloning
                state.Update(this, stats);
#if ALLOW_STATE_PROFILING
                //TODO keep record of all stat changes.
#endif
            }

            ProcessQueues();
            ProcessActionBuffer();
        }

        public bool Is<TState>() => statesStack.Contains(typeof(TState));

        private void OnStateChangeHandler(Actor actor)
        {
        }

        public void EnterState<T>() where T : State
        {
            EnterState(typeof(T));
        }

        public void EnterState<T>(Message initiator) where T : State
        {
            EnterState(typeof(T), initiator);
        }

        private void EnterState(Type stateType)
        {
            EnterState(stateType, null);
        }

        private void EnterState(Type stateType, Message initiator)
        {
            statePool.TryGetValue(stateType, out State pooledState);
            State newState = pooledState ?? (State)Activator.CreateInstance(stateType);
            if (newState.Group > -1) ExitStateGroup(newState.Group);

            StateAndInitiator si = new(
                newState, initiator
            );

            slatedForCreation.Enqueue(si);
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
                TearDownState(state);
                state.End(this);
                changed = true;
            }

            while (slatedForCreation.Count > 0)
            {
                //TODO extract methods here.
                StateAndInitiator si = slatedForCreation.Dequeue();
                State newState = si.state;
                //TODO Extract these following methods to -> ResolveDependencyStates.  
                if (!HasRequirements(newState)) continue;
                foreach (Type negatedState in newState.negatedStates) ExitState(negatedState);
                foreach (Type partnerState in newState.partnerStates) EnterState(partnerState);
                if (newState.solo) ExitAllStatesExcept(newState);
                statesStack.Add(newState);
                statePool.Remove(newState.GetType());
                BuildState(newState);
                newState.Init(this, si.initiator);
                newState.time = 0;
                changed = true;
            }

            if (changed && OnStateChange != null)
                OnStateChange(this);
        }

        private void BuildState(State newState)
        {
            newState.OnExit += HandleStateExit;
        }

        private void TearDownState(State state)
        {
            state.OnExit -= HandleStateExit;
        }

        private void HandleStateExit(State state) => ExitState(state);

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

            Message firstMessage = actionBuffer.Peek();
            firstMessage.timer += Time.deltaTime;

            if (PropagateAction(firstMessage, false))
            {
                actionBuffer.Dequeue();
            }
            else if (firstMessage.timer >= actionTimer)
            {
                actionBuffer.Dequeue();
            }
        }

        protected bool PropagateAction(Message message, bool buffer = true)
        {
            if (statesStack.Count < 1)
            {
                throw new Exception("This Actor has no states!");
            }

            foreach (State s in statesStack)
            {
                if (s.Process(this, message)) break;
            }

            if (ShouldBufferMessage(message, buffer))
                actionBuffer.Enqueue(message);

            return message.processed;
        }

        private static bool ShouldBufferMessage(Message message, bool buffer) =>
            !message.processed && buffer && message.phase == Message.Phase.Started;

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
            public readonly Message initiator;

            public StateAndInitiator(State state, Message initiator)
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