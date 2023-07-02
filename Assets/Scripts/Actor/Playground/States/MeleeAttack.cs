using CSM;
using JetBrains.Annotations;

namespace playground
{
    [UsedImplicitly]
    [StateDescriptor(group = 3, priority = 5)]
    public class MeleeAttack : State
    {
        private int combo;

        public override void Init(Actor actor, Message initiator)
        {
            combo = 0;
        }

        public override Stats? Update(Actor actor, Stats stats)
        {
            if (time >= .5f) Exit();
            return null;
        }

        public override bool Process(Actor actor, Message message)
        {
            if (message.phase == Message.Phase.Started)
            {
                if (message.name == "Attack")
                {
                    message.processed = true;
                    combo++;
                }
            }

            return true;
        }
    }
}