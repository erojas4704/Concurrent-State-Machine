using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Actor.States.Entity;

namespace Actor
{
    public class Actor : MonoBehaviour, ISerializationCallbackReceiver
    {
        private StateSet states = new StateSet(new StateComparer());
        private Queue<State> slatedForDeletion = new Queue<State>();
        private Queue<State> slatedForCreation = new Queue<State>();
        void Start()
        {
            EnterState(typeof(Airborne));
        }

        void Update()
        {
            foreach (State state in states) state.Update(this);
            processQueues();

            slatedForDeletion.Clear();
            slatedForCreation.Clear();

            if (Input.anyKey)
            {
                states.Min.Process(this, new Action { name = "jump" });
            }
        }

        private void EnterState(Type stateType)
        {
            State newState = (State)Activator.CreateInstance(stateType);
            ExitStateGroup(newState.Group);

            slatedForCreation.Enqueue(newState);
            newState.Enter = EnterState;
            newState.Exit = ExitState;
            newState.Init(this);
            UpdateStateChain();
        }

        private void ExitState(State state)
        {
            slatedForDeletion.Enqueue(state);
            state.End(this);
            UpdateStateChain();
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
            while (slatedForDeletion.Count > 0)
                states.Remove(slatedForDeletion.Dequeue());

            while (slatedForCreation.Count > 0)
                states.Add(slatedForCreation.Dequeue());
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