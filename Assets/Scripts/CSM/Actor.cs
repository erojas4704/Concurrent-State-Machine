using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace CSM
{
    [RequireComponent(typeof(Stats))]
    public class Actor : MonoBehaviour, ISerializationCallbackReceiver
    {
        private StateStack statesStack = new StateStack();

        /**States scheduled to be deleted this frame. This work is done at the end of an update cycle.*/
        private Queue<State> slatedForDeletion = new Queue<State>();

        /**States scheduled to be initialized this frame. This work is done at the end of an update cycle. */
        private Queue<StateAndInitiator> slatedForCreation = new Queue<StateAndInitiator>();

        /** Pool of states that have been removed. This prevents GC running on expired states. */
        private readonly Dictionary<Type, State> statePool = new Dictionary<Type, State>();

        private readonly Dictionary<Type, GhostState> ghostStates = new Dictionary<Type, GhostState>();

        [SerializeField, HideInInspector] private string defaultState;

        //TODO <- This may be better off delegated to a persistent stat. 
        public Vector3 velocity;
        private Stats stats;
        public delegate void StateChangeHandler(Actor actor);
        public event StateChangeHandler OnStateChange;
        private readonly MessageBroker messageBroker = new MessageBroker();


        private void Awake()
        {
            stats = GetComponent<Stats>();
            EnterDefaultState();
        }

        public virtual void Update()
        {
            if (stats) stats.Reset();

            messageBroker.PrimeMessages();
            foreach (State state in statesStack)
            {
                messageBroker.ProcessMessagesForState(state);
                state.Update();
                //TODO [Z-56] keep record of all stat changes.
            }
            messageBroker.CleanUp();

            ProcessQueues();
        }

        //TODO Z-67: What happens if we call this with a state that does not exist in the stack?
        public T GetState<T>() where T : State => (T)statesStack[typeof(T)];

        public bool Is<TState>() => statesStack.Contains(typeof(TState));

        public StateStack GetStates()
        {
            return statesStack;
        }

        private void OnStateChangeHandler(Actor actor) { }

        private void EnterState(Type stateType, Message initiator)
        {
            if (!stateType.IsSubclassOf(typeof(State)))
            {
                throw new CsmException("Actor {name} tried to enter invalid state {stateType}.");
            }

            //TODO Z-67 pending new StateRelationship. For now we brute force it. This is wildly inefficient. O(N^2)
            foreach (State state in statesStack)
            {
                if (state.negatedStates.Contains(stateType))
                {
                    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                    Debug.LogWarning(
                        $"Actor {name} tried to enter state {stateType}, but it is negated by state {state}.");
                    return;
                }
            }

            State newState = GetOrCreateState(stateType);

            StateAndInitiator si = new StateAndInitiator(newState, initiator);

            slatedForCreation.Enqueue(si);

            //TODO Z-67. Wildly inefficient use of queues. Redesign the list.
            //If the state we are creating is slated for deletion, we make sure it isn't.
            slatedForDeletion =
                new Queue<State>(slatedForDeletion.ToList().Where(state => state.GetType() != stateType));
        }

        private State GetOrCreateState(Type stateType)
        {
            statePool.TryGetValue(stateType, out State pooledState);
            if (pooledState != null)
            {
                return pooledState;
            }

            State newState = (State)Activator.CreateInstance(stateType);
            newState.ValidateRequirements();
            return newState;
        }

        private void ExitState(State state)
        {
            slatedForDeletion.Enqueue(state);
            //If this state is in the addStateQueue, short-circuit it.
        }


        /**Creates and deletes all states that are slated for creation or deletion. This method handles state pooling and
         * resolving dependencies.
         */
        private void ProcessQueues()
        {
            bool changed = false;

            while (slatedForCreation.Count > 0)
            {
                if (HasSoloState())
                {
                    slatedForCreation.Clear();
                    break;
                }

                StateAndInitiator si = slatedForCreation.Dequeue();
                if (statesStack.Contains(si.state)) continue;

                List<State> statesToCreate = new List<State> { si.state };
                List<State> statesToDestroy = new List<State>();

                HashSet<Type> stateTypesProcessed = new HashSet<Type>();

                if (ResolveStateDependencies(si.state, ref statesToCreate, ref statesToDestroy,
                        ref stateTypesProcessed))
                {
                    if (statesToCreate.Intersect(statesToDestroy).Any())
                    {
                        throw new CsmException(
                            "Invalid state configuration. Unsolvable dependency chain! Dependent states are being negated by their dependencies!",
                            si.state.GetType());
                    }

                    if (si.state.solo)
                    {
                        //Destroy absolutely everything else and terminate the loop
                        foreach (State negatedBySolo in statesStack.Values)
                        {
                            slatedForDeletion.Enqueue(negatedBySolo);
                        }

                        slatedForCreation.Clear();
                    }

                    changed = true;
                    foreach (State stateToCreate in statesToCreate)
                    {
                        CreateState(stateToCreate, si.initiator);
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

            if (changed)
            {
                OnStateChange?.Invoke(this);
                if (statesStack.Count < 1) EnterDefaultState();
            }
        }

        private bool HasSoloState() => statesStack.Values.Any(state => state.solo);


        private State CreateState(State newState, Message initiator = null)
        {
            statesStack.Add(newState);
            StateSetup(newState);
            newState.Init(initiator);
            newState.startTime = Time.time;
            newState.expiresAt = 0;
            return newState;
        }

        private bool ResolveStateDependencies(State newState, ref List<State> statesToCreate,
            ref List<State> statesToDestroy, ref HashSet<Type> stateTypesProcessed)
        {
            //TODO Z-67 This method is spaghetti 
            //Avoids circular dependencies.
            if (stateTypesProcessed.Contains(newState.GetType()))
            {
                return true;
            }

            if (newState.Group > -1)
            {
                //TODO Z-67, Implement a method to get grouped state from StateStack.
                foreach (State state in statesStack)
                {
                    if (state.Group == newState.Group)
                    {
                        statesToDestroy.Add(state);
                    }
                }
            }

            stateTypesProcessed.Add(newState.GetType());


            //Check incoming states to see if any of our dependencies are there.
            //TODO Z-62 clean this unholy godawful method up
            foreach (StateAndInitiator stateAndInitiator in slatedForCreation)
            {
                foreach (Type requiredStateType in newState.requiredStates)
                {
                    if (stateAndInitiator.state.GetType() == requiredStateType)
                    {
                        State requiredState = GetOrCreateState(requiredStateType);
                        if (ResolveStateDependencies(requiredState, ref statesToCreate,
                                ref statesToDestroy, ref stateTypesProcessed))
                        {
                            statesToCreate.Add(requiredState);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            if (!ActorHasRequiredStatesFor(newState, statesToCreate, stateTypesProcessed))
            {
                return false;
            }

            foreach (Type partnerStateType in newState.partnerStates)
            {
                if (statesStack.Contains(partnerStateType))
                {
                    continue;
                }

                State newPartnerState = GetOrCreateState(partnerStateType);
                if (ResolveStateDependencies(newPartnerState, ref statesToCreate, ref statesToDestroy,
                        ref stateTypesProcessed))
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
                if (newState.GetType() == negatedStateType ||
                    statesToCreate.Any(state => state.GetType() == negatedStateType))
                {
                    throw new CsmException(
                        "Invalid state configuration. Unsolvable dependency chain! Dependent states are being negated by their dependencies!",
                        newState.GetType());
                }


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

        private void HandleStateExit(State state, float persistDuration = 0f, params string[] messagesToListenFor)
        {
            if (persistDuration > 0f)
            {
                state.expiresAt = Time.time + persistDuration;
                GhostState ghostState = new GhostState
                {
                    state = state,
                    messagesToListenFor = new HashSet<string>(messagesToListenFor)
                };

                ghostStates[state.GetType()] = ghostState;
            }

            ExitState(state.GetType());
        }


        // ReSharper disable Unity.PerformanceAnalysis
        private bool ActorHasRequiredStatesFor(State state, List<State> statesCreatedThisFrame,
            HashSet<Type> statesProcessedRecursively) //statesProcessedRecursively added as a HACK
        {
            foreach (Type requiredState in state.requiredStates)
            {
                bool requirementExists;
                requirementExists = statesStack.Contains(requiredState) ||
                                    statesProcessedRecursively.Contains(requiredState);

                //TODO Z-67... you already know
                foreach (State incomingState in statesCreatedThisFrame)
                    if (incomingState.GetType() == requiredState)
                    {
                        requirementExists = true;
                        break;
                    }

                if (requirementExists)
                    continue;

                Debug.LogWarning($"Not entering state {state} because dependency {requiredState} is missing!");
                return false;
            }

            return true;
        }

        public void PropagateMessage(Message message, bool buffer = true)
        {
            message.isBufferable = buffer;
            messageBroker.EnqueueMessage(message);
        }

        public bool PropagateMessage2(Message message, bool buffer = true)
        {
            foreach (State s in statesStack)
            {
                bool messageBlocked = s.Process(message);
                if (messageBlocked) return message.processed;
            }

            //TODO [Z-67]...
            //Process ghost states. Ghost states have no order and cannot block messages.
            GhostState[] ghostStateValues = ghostStates.Values.ToArray();
            foreach (GhostState ghost in ghostStateValues)
            {
                if (ghost.messagesToListenFor.Count > 0 && !ghost.messagesToListenFor.Contains(message.name)) continue;
                State ghostState = ghost.state;
                if (statesStack.Contains(ghost.state.GetType()))
                {
                    ghostStates.Remove(ghostState.GetType());
                    continue;
                }

                if (Time.time < ghostState.expiresAt)
                {
                    ghostState.Process(message);
                    if (message.processed)
                    {
                        ghostStates.Remove(ghostState.GetType());
                    }
                }
            }

            return message.processed;
        }


        private List<State> GetDependentStates(State state)
        {
            List<State> dependentStates = new List<State>();
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

        private void EnterDefaultState()
        {
            if (string.IsNullOrEmpty(defaultState)) return;
            Type defaultStateType = ActorUtils.FindType(defaultState);
            if (defaultStateType == null)
            {
                throw new CsmException($"Default state for {gameObject.name} is invalid!");
            }

            EnterState(defaultStateType);
        }

        private class StateAndInitiator
        {
            public readonly State state;
            public readonly Message initiator;

            public StateAndInitiator(State state, Message initiator = null)
            {
                this.state = state;
                this.initiator = initiator;
            }
        }

        private class GhostState
        {
            public State state;
            public HashSet<string> messagesToListenFor;
        }

        #region ExitState overloads

        public void ExitState(Type stateType)
        {
            if (statesStack.TryGetValue(stateType, out State state))
            {
                ExitState(state);
            }

            //TODO Completely Refactor Queuing System. This is wildly inefficient.
            slatedForCreation = new Queue<StateAndInitiator>(slatedForCreation.ToList()
                .Where(stateAndInitiator => stateAndInitiator.state.GetType() != stateType).ToList());
        }

        public void ExitState<T>() where T : State => ExitState(typeof(T));

        #endregion

        #region EnterState overloads

        public void EnterState<T>(object initiator) where T : State => EnterState<T>(new Message("", initiator));

        public void EnterState<T>() where T : State => EnterState(typeof(T));

        public void EnterState<T>(Message initiator) where T : State
        {
            EnterState(typeof(T), initiator);
        }

        public void EnterState(Type stateType)
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