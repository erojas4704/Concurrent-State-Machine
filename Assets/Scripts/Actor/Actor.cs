using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor.Entity;

namespace Actor
{
    public class Actor : MonoBehaviour, ISerializationCallbackReceiver
    {
        private SortedSet<State> states = new SortedSet<State>();
        void Start()
        {
            EnterState<Airborne>();
        }

        void Update()
        {
            foreach (State state in states)
            {
                state.Update(this);
            }


            if (Input.anyKey)
            {
                Debug.Log("Should do SOMETHING");
                states.Min.Process(this, new Action { name = "jump" });
            }
        }

        private void EnterState<TState>() where TState : new()
        {
            State newState = new TState() as State;
            states.Add(newState);

            foreach (State s in states)
            {

            }


            newState.Enter = EnterState<TState>;
            newState.Exit = ExitState<TState>;
            newState.Init(this);
        }

        private void ExitState<TState>()
        {

        }

        private void UpdateStateChain(State newState)
        {
            State prev = null;
            State current = null;
            foreach (State s in states)
            {
                if (s == newState)
                {
                    current = s;
                    if (prev != null)
                        prev.Next = current.Process;
                }

                if (prev == newState)
                {
                    if (current != null)
                        current.Next = s.Process;
                }

                prev = s;
            }
        }

        private State[] _stateArray;
        public void OnBeforeSerialize()
        {
            _stateArray = new State[states.Count];
            states.CopyTo(_stateArray);
        }
        public void OnAfterDeserialize()
        {
            states = new SortedSet<State>(_stateArray);
        }
    }
}