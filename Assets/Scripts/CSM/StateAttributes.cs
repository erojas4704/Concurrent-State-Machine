using System;

namespace csm
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class StateDescriptor : System.Attribute
    {
        public int group = -1;
        public int priority;

        public StateDescriptor()
        {
        }
    }

    public class Negate : System.Attribute
    {
        public Type[] states;

        public Negate(params Type[] states)
        {
            this.states = states;
        }
    }

    public class Require : System.Attribute
    {
        public Type[] states;

        public Require(params Type[] states)
        {
            this.states = states;
        }
    }

    public class With : System.Attribute
    {
        public Type[] states;

        public With(params Type[] states)
        {
            this.states = states;
        }
    }

    public class Solo : System.Attribute
    {
        public bool solo;

        public Solo(bool solo = true)
        {
            this.solo = solo;
        }
    }
}