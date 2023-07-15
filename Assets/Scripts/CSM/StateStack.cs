using System;
using System.Collections;
using System.Collections.Generic;

namespace CSM
{
    public class StateStack : IList<State>, IDictionary<Type, State>
    {
        private readonly Dictionary<Type, State> dictionary;
        private readonly List<State> list;

        public StateStack()
        {
            IsReadOnly = true;
            dictionary = new();
            list = new();
        }

        IEnumerator<KeyValuePair<Type, State>> IEnumerable<KeyValuePair<Type, State>>.GetEnumerator() =>
            dictionary.GetEnumerator();

        public IEnumerator<State> GetEnumerator() => list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

        //TODO Z-67: Add in a clause that checks state groups. We will have a separate list for groupings and methods to extract them easily. 
        public void Add(State newState)
        {
            if (newState == null) throw new CsmException("Actor trying to enter null state");
            if (dictionary.ContainsKey(newState.GetType()))
                return;

            InsertStateIntoList(newState);
            dictionary[newState.GetType()] = newState;
        }

        public void Clear()
        {
            dictionary.Clear();
            list.Clear();
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

        public void Add(KeyValuePair<Type, State> item)
        {
            throw new NotImplementedException();
        }

        public ICollection<Type> Keys => dictionary.Keys;
        public ICollection<State> Values => list;

        public bool Remove(State item) => item != null && Remove(item.GetType());

        public int Count => list.Count;
        public bool IsReadOnly { get; }

        private void InsertStateIntoList(State newState)
        {
            if (list.Count < 1)
            {
                list.Add(newState);
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (newState.Priority >= list[i].Priority)
                {
                    list.Insert(i, newState);
                    return;
                }
            }

            list.Add(newState);
        }

        #region Unimplemented Methods

        public bool Contains(KeyValuePair<Type, State> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<Type, State>[] array, int arrayIndex)
        {
            int i = 0;
            while (i++ < list.Count - 1)
            {
                array[i + arrayIndex] = new(list[i].GetType(), list[i]);
            }
        }

        public bool Remove(KeyValuePair<Type, State> item)
        {
            throw new NotImplementedException();
        }

        public void Add(Type key, State value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(Type key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(Type key, out State value) => dictionary.TryGetValue(key, out value);

        public State this[Type key]
        {
            get => dictionary[key];
            set => throw new NotImplementedException();
        }

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

        #endregion
    }
}