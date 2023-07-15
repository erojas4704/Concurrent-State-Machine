using System;

namespace CSM
{
    public class CsmException : Exception
    {
        private Type triggerState;

        public CsmException(string message) : base(message) { }

        public CsmException(string message, Type triggerState) : base(message)
        {
            this.triggerState = triggerState;
        }

        public CsmException(string message, State triggerState) : this(message, triggerState.GetType())
        {
            this.triggerState = triggerState.GetType();
        }
    }
}