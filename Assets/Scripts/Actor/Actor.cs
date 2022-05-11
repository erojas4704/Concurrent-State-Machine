using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using CSM.States;

namespace CSM
{
    public class Actor : MonoBehaviour, ISerializationCallbackReceiver
    {
        private StateSet states = new StateSet(new StateComparer());
        private Queue<State> slatedForDeletion = new Queue<State>();
        private Queue<State> slatedForCreation = new Queue<State>();

        private HashSet<State> statePool = new HashSet<State>();
        void Start()
        {
        }

        void Update()
        {
            foreach (State state in states) state.Update(this);
            processQueues();
        }

        public void EnterState<T>() where T : State
        {
            EnterState(typeof(T));
        }

        public void EnterState(Type stateType)
        {
            State pooledState = statePool.FirstOrDefault<State>(s => s.GetType() == stateType);
            State newState = pooledState != null ? pooledState : (State)Activator.CreateInstance(stateType);
            ExitStateGroup(newState.Group);
            slatedForCreation.Enqueue(newState);
        }

        private void ExitState(State state)
        {
            slatedForDeletion.Enqueue(state);
        }

        private void ExitState(Type stateType)
        {
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
            while (slatedForDeletion.Count > 0)//
            {
                State state = slatedForDeletion.Dequeue();
                states.Remove(state);
                statePool.Add(state);
                state.End(this);
                UpdateStateChain();
            }

            while (slatedForCreation.Count > 0)
            {
                State newState = slatedForCreation.Dequeue();
                states.Add(newState);
                statePool.Remove(newState);
                newState.Enter = EnterState;
                newState.Exit = ExitState;
                newState.Init(this);
                UpdateStateChain();
            }
        }

        public void FireAction(Action action)
        {
            states.Min.Process(this, action);
        }

        private void ExitStateGroup(int group)
        {
            foreach (State state in states)
                if (state.Group == group)
                    ExitState(state.GetType());
        }

        private void UpdateStateChain()
        {
            State prev = null;
            foreach (State s in states)
            {
                if (prev != null)
                {
                    prev.Next = s.Process;
                }

                prev = s;
            }
        }

        public StateSet GetStates()
        {
            return states;
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
        }
        #endregion
    }
}