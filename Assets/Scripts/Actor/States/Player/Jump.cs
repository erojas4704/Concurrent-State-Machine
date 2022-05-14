namespace CSM.States
{
    [StateDescriptor(priority = 2, group = 2)]
    public class Jump : State
    {
        private Entity entity;
        private bool isHeld;
        private float hangTime = 0.55f;

        override public void Init(Actor actor)
        {
            entity = actor.GetComponent<Entity>();
            entity.Velocity.y = 5f;
        }

        override public void Update(Actor actor)
        {
            if (entity.Velocity.y < 0)
            {
                if (isHeld && time < hangTime)
                    entity.Velocity.y = 0f;
                else
                    Exit(this.GetType());
            }
        }

        override public void Process(Actor actor, Action action)
        {
            if (action.name == "Jump" && action.phase == Action.ActionPhase.Held)
            {
                isHeld = true;
            }
            else if (action.name == "Jump" && action.phase == Action.ActionPhase.Released)
                isHeld = false;

            Next(actor, action);
        }

        override public void End(Actor actor)
        {
        }

    }
}