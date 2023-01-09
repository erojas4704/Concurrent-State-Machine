using System.Collections.Generic;
using System;

namespace csm
{
    public class StateSet : SortedSet<State>
    {
        public StateSet() : base(new StateComparer()) { }
        public StateSet(IComparer<State> comparer) : base(comparer) { }
        public StateSet(IEnumerable<State> collection) : base(collection, new StateComparer()) { }
        public StateSet(IEnumerable<State> collection, IComparer<State> comparer) : base(collection, comparer) { }

        public State RemoveType(Type stateType)
        {
            State state;
            foreach (State s in this)
            {
                if (s.GetType() == stateType)
                {
                    state = s;
                    Remove(state);
                    return state;
                }
            }
            return null;
        }
    }
}