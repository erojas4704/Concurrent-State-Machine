namespace CSM.States
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class StateDescriptor : System.Attribute
    {
        public string group;
        public int priority;

        public StateDescriptor()
        {
        }
    }
}