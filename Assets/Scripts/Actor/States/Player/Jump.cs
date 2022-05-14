using CSM.Entities;
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
            entity = (Entity)actor;
            entity.velocity.y = 9f;
        }

        override public void Update(Actor actor)
        {
            if (entity.velocity.y < 0)
            {
                if (isHeld && time < hangTime)
                    entity.velocity.y = 0f;
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