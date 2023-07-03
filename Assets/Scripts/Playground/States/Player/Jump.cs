using CSM;
using JetBrains.Annotations;

namespace playground
{
    [UsedImplicitly]
    [StateDescriptor(priority = 2, group = 2)]
    public class Jump : State
    {
        private bool isHeld;
        private const float HANG_TIME = 0.55f;

        public override void Init(Actor actor, Message initiator)
        {
            actor.velocity.y = 7.5f;
        }

        public override Stats? Update(Actor actor, Stats stats)
        {
            if (actor.velocity.y < 0)
            {
                if (isHeld && time < HANG_TIME)
                    actor.velocity.y = 0f;
                else
                    Exit();
            }

            return null;
        }

        public override bool Process(Actor actor, Message message)
        {
            if (message.name == "Jump" && message.phase == Message.Phase.Held)
            {
                isHeld = true;
            }
            else if (message.name == "Jump" && message.phase == Message.Phase.Ended)
                isHeld = false;

            return false;
        }
    }
}