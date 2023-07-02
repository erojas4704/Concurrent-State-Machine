using CSM;
using JetBrains.Annotations;

namespace playground
{
    [UsedImplicitly]
    [StateDescriptor(priority = 2, group = 2)]
    public class Jump : State
    {
        private Actor entity;
        private bool isHeld;
        private float hangTime = 0.55f;

        public override void Init(Actor actor, Message initiator)
        {
            entity = (Actor)actor;
            entity.velocity.y = 7.5f;
        }

        public override Stats? Update(Actor actor, Stats stats)
        {
            if (entity.velocity.y < 0)
            {
                if (isHeld && time < hangTime)
                    entity.velocity.y = 0f;
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

        public override void End(Actor actor)
        {
        }

    }
}