using CSM;
using JetBrains.Annotations;

namespace playground
{
    [UsedImplicitly]
    [StateDescriptor(priority = 4, group = 4)]
    public class MeleeArmed : State
    {
        public override bool Process(Actor actor, Action action)
        {
            if (action.phase == Action.ActionPhase.Pressed)
            {
                if (action.name == "Attack")
                {
                    action.processed = true;
                    actor.EnterState<MeleeAttack>();
                }
            }

            return false;
        }
    }
}