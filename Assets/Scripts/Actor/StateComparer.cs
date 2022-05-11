using System.Collections.Generic;

namespace CSM {
    public class StateComparer : IComparer<State> {
        public int Compare(State x, State y) {
            return y.priority.CompareTo(x.priority);
        }
    }
}