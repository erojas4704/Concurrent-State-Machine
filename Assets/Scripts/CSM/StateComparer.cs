using System.Collections.Generic;

namespace csm {
    public class StateComparer : IComparer<State> {
        public int Compare(State x, State y) {
            return y.priority.CompareTo(x.priority);
        }
    }
}