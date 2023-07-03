using CSM;
using JetBrains.Annotations;

namespace Playground.States
{
    [UsedImplicitly]
    [StateDescriptor(priority = 4, group = 4)]
    public class MeleeArmed : State
    {
        public override bool Process(Actor actor, Message message)
        {
            if (message.phase == Message.Phase.Started)
            {
                if (message.name == "Attack")
                {
                    message.processed = true;
                    actor.EnterState<MeleeAttack>();
                }
            }

            return false;
        }
    }
}