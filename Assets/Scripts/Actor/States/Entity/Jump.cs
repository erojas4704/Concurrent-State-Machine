namespace CSM.States
{
    public class Jump : State
    {
        private Entity entity;
        public Jump()
        {
            Group = 2;
            Priority = 2;
        }

        override public void Init(Actor actor)
        {
            entity = actor.GetComponent<Entity>();
            entity.Velocity.y = 4f;
        }

        override public void Update(Actor actor)
        {
            if (entity.Velocity.y < 0) Exit(this.GetType());
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