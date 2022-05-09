namespace Actor
{
    public class Grounded : State
    {
        override public void Init(Actor actor)
        {
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