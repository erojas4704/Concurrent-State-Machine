using System.Collections.Generic;
using System;
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
        private readonly Queue<Message> messageBuffer = new();

        /** Pool of states that have been removed. This prevents GC running on expired states. */
        private readonly Dictionary<Type, State> statePool = new();

        private List<State> ghostStates = new();

        //TODO <- This may be better off delegated to a persistent stat. 
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
            if (stats) stats.Reset();
            foreach (State state in statesStack)
            {
                state.time += Time.deltaTime;
                state.Update();
                //TODO Z-56 keep record of all stat changes.
            }

            ProcessQueues();
            ProcessActionBuffer();
        }

        /**Persist a state as a 'ghost'. Ghost states will be treated as the bottom of the stack but will still
         * be able to process messages. This is useful for implementing features like "coyote-time".
         */
        public void Persist(State state, float duration)
        {
            //TODO: z-52 Remove this method and take in a duration as part of a state's Exit() event method.
            //TODO: z-52 States can be persisted but not necessarily exited. This is a problem.
            state.expiresAt = Time.time + duration;

            if (!ghostStates.Contains(state))
            {
                ghostStates.Add(state);
            }
        }

        public bool Is<TState>() => statesStack.Contains(typeof(TState));

        public StateStack GetStates()
        {
            return statesStack;
        }

        private void OnStateChangeHandler(Actor actor)
        {
        }

        private void EnterState(Type stateType, Message initiator)
        {
            State newState = GetOrCreateState(stateType);
            if (newState.Group > -1)
                ExitStateGroup(newState.Group);
            //TODO <- This needs to be revised in SettleStateDependencies or whatever.

            StateAndInitiator si = new(
                newState, initiator
            );

            slatedForCreation.Enqueue(si);
        }

        private State GetOrCreateState(Type stateType)
        {
            statePool.TryGetValue(stateType, out State pooledState);
            return pooledState ?? (State)Activator.CreateInstance(stateType);
        }

        private void ExitState(State state)
        {
            slatedForDeletion.Enqueue(state);
        }

        /**Creates and deletes all states that are slated for creation or deletion. This method handles state pooling and
         * resolving dependencies.
         */
        private void ProcessQueues()
        {
            bool changed = false;

            while (slatedForCreation.Count > 0)
            {
                StateAndInitiator si = slatedForCreation.Dequeue();
                List<State> statesToCreate = new(),
                    statesToDestroy = new();

                if (ResolveStateDependencies(si.state, ref statesToCreate, ref statesToDestroy) &&
                    CreateState(si) != null)
                {
                    changed = true;
                    foreach (State partnerState in statesToCreate)
                    {
                        CreateState(partnerState);
                    }

                    foreach (State stateToDestroy in statesToDestroy)
                        slatedForDeletion.Enqueue(stateToDestroy);
                }
            }

            while (slatedForDeletion.Count > 0)
            {
                State state = slatedForDeletion.Dequeue();
                if (!statesStack.Contains(state))
                    continue;

                statesStack.Remove(state);
                statePool.TryAdd(state.GetType(), state);
                StateTeardown(state);
                state.End();

                //Find any states that require the removed state and remove them as well.
                foreach (State dependentState in GetDependentStates(state))
                    slatedForDeletion.Enqueue(dependentState);
                
                changed = true;
            }

            if (changed && OnStateChange != null)
                OnStateChange(this);
        }

        private State CreateState(State newState) => CreateState(new StateAndInitiator(newState, null));

        private State CreateState(StateAndInitiator si)
        {
            State newState = si.state;
            statesStack.Add(newState);
            StateSetup(newState);
            newState.Init(si.initiator);
            newState.time = 0;
            return newState;
        }

        private bool ResolveStateDependencies(State newState, ref List<State> statesToCreate,
            ref List<State> statesToDestroy)
        {
            if (newState.solo)
            {
                statesToDestroy.AddRange(statesStack.Values);
                return true;
            }

            if (!ActorHasRequiredStatesFor(newState))
            {
                return false;
            }


            //TODO detect circular dependencies and short-circuit them.
            foreach (Type partnerStateType in newState.partnerStates)
            {
                State newPartnerState = GetOrCreateState(partnerStateType);
                if (ResolveStateDependencies(newPartnerState, ref statesToCreate, ref statesToDestroy))
                {
                    statesToCreate.Add(newPartnerState);
                }
                else
                {
                    return false;
                }
            }

            foreach (Type negatedStateType in newState.negatedStates)
            {
                if (statesStack.TryGetValue(negatedStateType, out State negatedState))
                {
                    statesToDestroy.Add(negatedState);
                }
            }

            return true;
        }

        private void StateSetup(State newState)
        {
            newState.OnExit += HandleStateExit;
            newState.SetStats(stats);
            newState.actor = this;
        }

        private void StateTeardown(State state)
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
        private bool ActorHasRequiredStatesFor(State state)
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
            if (messageBuffer.Count < 1) return;

            Message firstMessage = messageBuffer.Peek();
            firstMessage.timer += Time.deltaTime;

            if (PropagateAction(firstMessage, false))
            {
                messageBuffer.Dequeue();
            }
            else if (firstMessage.timer >= actionTimer)
            {
                messageBuffer.Dequeue();
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
                bool messageBlocked = s.Process(message);
                if (messageBlocked) return message.processed;
            }

            //Process ghost states. Ghost states have no order and cannot block messages.
            List<State> ghostStatesNextFrame = new();
            foreach (State ghost in ghostStates)
            {
                ghost.Process(message);
                if (Time.time < ghost.expiresAt & !message.processed)
                {
                    ghostStatesNextFrame.Add(ghost);
                }
            }

            if (ShouldBufferMessage(message, buffer))
                messageBuffer.Enqueue(message);

            ghostStates = ghostStatesNextFrame;
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

        private List<State> GetDependentStates(State state)
        {
            List<State> dependentStates = new();
            foreach (State activeState in statesStack)
            {
                if (activeState.requiredStates.Contains(state.GetType()) ||
                    activeState.partnerStates.Contains(state.GetType()))
                {
                    dependentStates.Add(activeState);
                }
            }

            return dependentStates;
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

        #region ExitState overloads

        private void ExitState(Type stateType)
        {
            State state = statesStack[stateType];
            if (state != null) ExitState(state);
        }

        public void ExitState<T>() where T : State => ExitState(typeof(T));

        #endregion

        #region EnterState overloads

        public void EnterState<T>() where T : State => EnterState(typeof(T));

        public void EnterState<T>(Message initiator) where T : State
        {
            EnterState(typeof(T), initiator);
        }

        private void EnterState(Type stateType)
        {
            EnterState(stateType, null);
        }

        #endregion

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