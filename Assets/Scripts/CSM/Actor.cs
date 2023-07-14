using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace CSM
{
    [RequireComponent(typeof(Stats))]
    public class Actor : MonoBehaviour, ISerializationCallbackReceiver
    {
        private StateStack statesStack = new();

        /**States scheduled to be deleted this frame. This work is done at the end of an update cycle.*/
        private Queue<State> slatedForDeletion = new();

        /**States scheduled to be initialized this frame. This work is done at the end of an update cycle. */
        private Queue<StateAndInitiator> slatedForCreation = new();

        /** Buffered messages for player input buffering. */
        private readonly Queue<Message> messageBuffer = new();

        /** Pool of states that have been removed. This prevents GC running on expired states. */
        private readonly Dictionary<Type, State> statePool = new();

        private List<State> ghostStates = new();

        private readonly Dictionary<string, Message> heldMessages = new();

        //TODO <- This may be better off delegated to a persistent stat. 
        public Vector3 velocity;
        private Stats stats;

        public delegate void StateChangeHandler(Actor actor);

        public event StateChangeHandler OnStateChange;

        /**How long messages are buffered for, in seconds. */
        public float messageBufferDurationSeconds = 0.05f;

        private void Awake()
        {
            stats = GetComponent<Stats>();
        }

        public virtual void Update()
        {
            if (stats) stats.Reset();

            ProcessQueues();
            ProcessActionBuffer();

            foreach (State state in statesStack)
            {
                state.Update();
                //TODO Z-56 keep record of all stat changes.
            }
        }

        //TODO Z-67: What happens if we call this with a state that does not exist in the stack?
        public T GetState<T>() where T : State => (T)statesStack[typeof(T)];

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

        private void OnStateChangeHandler(Actor actor) { }

        private void EnterState(Type stateType, Message initiator)
        {
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

            StateAndInitiator si = new(
                newState, initiator
            );

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
                StateAndInitiator si = slatedForCreation.Dequeue();
                if (statesStack.Contains(si.state)) continue;

                List<State> statesToCreate = new(),
                    statesToDestroy = new();

                HashSet<Type> stateTypesProcessed = new();

                if (ResolveStateDependencies(si.state, ref statesToCreate, ref statesToDestroy,
                        ref stateTypesProcessed) &&
                    CreateState(si) != null)
                {
                    if (statesToCreate.Intersect(statesToDestroy).Any())
                    {
                        throw new CsmException(
                            "Invalid state configuration. Unsolvable dependency chain! Dependent states are being negated by their dependencies!",
                            si.state.GetType());
                    }

                    changed = true;
                    foreach (State stateToCreate in statesToCreate)
                    {
                        CreateState(stateToCreate);
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
                Message[] messageArray = heldMessages.Values.ToArray();
                foreach (Message heldMessage in messageArray) PropagateMessage(heldMessage);
            }
        }

        private State CreateState(State newState) => CreateState(new StateAndInitiator(newState, null));

        private State CreateState(StateAndInitiator si)
        {
            State newState = si.state;
            statesStack.Add(newState);
            StateSetup(newState);
            newState.Init(si.initiator);
            newState.startTime = Time.time;
            newState.expiresAt = 0;
            //TODO Z-67: Nasty way of handling. Also potential InvalidOperationException. Break this down into a method
            foreach (Message message in heldMessages.Values) newState.Process(message); //Process held messages
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

            if (newState.solo)
            {
                statesToDestroy.AddRange(statesStack.Values);
                return true;
            }

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

            if (!ActorHasRequiredStatesFor(newState, statesToCreate))
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

        private void HandleStateExit(State state) => ExitState(state.GetType());


        // ReSharper disable Unity.PerformanceAnalysis
        private bool ActorHasRequiredStatesFor(State state, List<State> statesCreatedThisFrame)
        {
            foreach (Type requiredState in state.requiredStates)
            {
                bool requirementExists = false;
                requirementExists = statesStack.Contains(requiredState);

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

        private void ProcessActionBuffer()
        {
            if (messageBuffer.Count < 1) return;

            Message firstMessage = messageBuffer.Peek();
            firstMessage.timer += Time.deltaTime; //TODO <- make a field for this that uses Time.time instead of having to increment it every time.

            if (PropagateMessage(firstMessage, false))
            {
                messageBuffer.Dequeue();
            }
            else if (firstMessage.timer >= messageBufferDurationSeconds)
            {
                messageBuffer.Dequeue();
            }
        }

        public bool PropagateMessage(Message message, bool buffer = true)
        {
            if (statesStack.Count < 1)
            {
                throw new("This Actor has no states!");
            }
            
            if (message.phase == Message.Phase.Ended)
            {
                heldMessages.Remove(message.name);
            }

            foreach (State s in statesStack)
            {
                bool messageBlocked = s.Process(message);
                if (messageBlocked) return message.processed;
            }

            if (message.phase == Message.Phase.Held)
            {
                heldMessages[message.name] = message;
            }

            //Process ghost states. Ghost states have no order and cannot block messages.
            if (message.phase == Message.Phase.Started)
            {
                List<State> ghostStatesNextFrame = new();
                foreach (State ghost in ghostStates)
                {
                    if (Time.time < ghost.expiresAt)
                    {
                        ghost.Process(message);
                        if (!message.processed)
                        {
                            ghostStatesNextFrame.Add(ghost);
                        }
                    }
                }
                
                ghostStates = ghostStatesNextFrame;
            }

            if (ShouldBufferMessage(message, buffer))
                messageBuffer.Enqueue(message);

            return message.processed;
        }

        private static bool ShouldBufferMessage(Message message, bool buffer) =>
            !message.processed && buffer && message.phase == Message.Phase.Started;

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