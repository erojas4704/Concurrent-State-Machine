namespace CSM.States
{
    [StateDescriptor(group = "Melee", priority = 1)]
    public class MeleeAttack : State
    {
        private int combo;
        public MeleeAttack()
        {
            priority = 5;
            group = 3;
        }

        override public void Init(Actor actor)
        {
            combo = 0;
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
                    combo++;
                    return; //Eat the input so it doesn't trigger the next state
                }
            }

            Next(actor, action);
        }

        override public void End(Actor actor)
        {
        }

    }
}