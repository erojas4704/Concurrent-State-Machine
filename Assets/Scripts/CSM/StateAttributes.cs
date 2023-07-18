using System;

namespace CSM
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StateDescriptor : Attribute
    {
        public int group = -1;
        public int priority;
        public bool hidden;

        public StateDescriptor()
        {
        }
    }

    public class Negate : Attribute
    {
        public Type[] states;

        public Negate(params Type[] states)
        {
            this.states = states;
        }
    }

    public class Require : Attribute
    {
        public Type[] states;

        public Require(params Type[] states)
        {
            this.states = states;
        }
    }

    public class With : Attribute
    {
        public Type[] states;

        public With(params Type[] states)
        {
            this.states = states;
        }
    }

    public class Solo : Attribute
    {
        public bool solo;

        public Solo(bool solo = true)
        {
            this.solo = solo;
        }
    }
}