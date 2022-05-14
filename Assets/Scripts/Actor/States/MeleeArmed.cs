namespace CSM.States
{
    [StateDescriptor(priority = 4, group = 4)]
    public class MeleeArmed : State
    {
        override public void Init(Actor actor)
        {
        }

        override public void Update(Actor actor)
        {
        }

        override public void Process(Actor actor, Action action)
        {
            if (action.phase == Action.ActionPhase.Pressed)
            {
                if (action.name == "Attack")
                {
                    action.processed = true;
                    actor.EnterState<MeleeAttack>();
                    return;
                }
            }
            Next(actor, action);
        }

        override public void End(Actor actor)
        {
        }

    }
}