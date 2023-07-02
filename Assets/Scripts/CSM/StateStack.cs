using System.Collections.Generic;
using System;
using System.Collections;

namespace CSM
{
    public class StateStack : IList<State>, IDictionary<Type, State>
    {
        private readonly Dictionary<Type, State> dictionary;
        private readonly List<State> list;

        public StateStack()
        {
            IsReadOnly = true;
            dictionary = new Dictionary<Type, State>();
            list = new List<State>();
        }

        IEnumerator<KeyValuePair<Type, State>> IEnumerable<KeyValuePair<Type, State>>.GetEnumerator() =>
            dictionary.GetEnumerator();

        public IEnumerator<State> GetEnumerator() => list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

        public void Add(State newState)
        {
            if (newState == null) throw new NullReferenceException("Actor trying to enter null state");
            if (dictionary.ContainsKey(newState.GetType()))
                return;

            InsertStateIntoList(newState);
            dictionary[newState.GetType()] = newState;
        }

        public void Add(KeyValuePair<Type, State> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            dictionary.Clear();
            list.Clear();
        }

        public bool Contains(KeyValuePair<Type, State> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<Type, State>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<Type, State> item)
        {
            throw new NotImplementedException();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public bool Contains(Type stateType) => dictionary.ContainsKey(stateType);
        public bool Contains(State state) => state != null && Contains(state.GetType());

        public void CopyTo(State[] array, int arrayIndex = 0)
        {
            for (int i = arrayIndex; i < list.Count; i++)
            {
                array[i - arrayIndex] = list[i];
            }
        }

        //Slow. O(N).
        public void Add(Type key, State value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(Type key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Type stateType)
        {
            if (!dictionary.ContainsKey(stateType))
            {
                throw new Exception($"Tried to remove non-existent state {stateType} from stack.");
            }

            for (int i = 0; i < list.Count; i++)
            {
                State s = list[i];
                if (s.GetType() != stateType) continue;
                list.RemoveAt(i);
                dictionary.Remove(stateType);
                return true;
            }

            return false;
        }

        public bool TryGetValue(Type key, out State value)
        {
            throw new NotImplementedException();
        }

        public State this[Type key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public ICollection<Type> Keys => dictionary.Keys;
        public ICollection<State> Values => dictionary.Values;

        public bool Remove(State item) => item != null && Remove(item.GetType());

        public int Count => list.Count;
        public bool IsReadOnly { get; }

        public int IndexOf(State item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, State item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public State this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        //TODO unit test
        private void InsertStateIntoList(State newState)
        {
            if (list.Count < 1)
            {
                list.Add(newState);
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (i == list.Count - 1)
                {
                    list.Add(newState);
                    break;
                }

                State nextState = list[i + 1];
                if (i == 0 && newState.Priority > nextState.Priority)
                {
                    list.Insert(0, newState);
                    break;
                }

                State lastState = list[i - 1];
                if (newState.Priority <= lastState.Priority || newState.Priority >= nextState.Priority) continue;
                list.Insert(i, newState);
                break;
            }
        }
    }
}