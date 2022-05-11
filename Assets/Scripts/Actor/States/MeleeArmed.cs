namespace CSM.States
{
    public class MeleeArmed : State
    {
        public MeleeArmed()
        {
            priority = 4;
            group = 4;
        }

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