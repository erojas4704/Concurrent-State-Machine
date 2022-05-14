using CSM.Entities.States;

namespace CSM.States
{
    [StateDescriptor(priority = 4, group = 4)]
    public class MeleeArmed : State
    {

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

    }
}