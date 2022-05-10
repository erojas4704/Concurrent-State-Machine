namespace CSM.States
{
    [System.Serializable]

    public class Grounded : State
    {
        public Grounded(){
            Group = 0;
            Priority = 1;
        }

        override public void Init(Actor actor)
        {
            base.Init(actor);
        }

        override public void Update(Actor actor)
        {
        }

        override public void Process(Actor actor, Action action)
        {
            Next(actor, action);
        }

        override public void End(Actor actor)
        {
        }

    }
}