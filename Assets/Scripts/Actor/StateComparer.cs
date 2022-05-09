using System.Collections.Generic;

namespace Actor {
    public class StateComparer : IComparer<State> {
        public int Compare(State x, State y) {
            return x.Priority.CompareTo(y.Priority);
        }
    }
}