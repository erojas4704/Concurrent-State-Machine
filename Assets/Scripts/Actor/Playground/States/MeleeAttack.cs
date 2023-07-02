using CSM;
using JetBrains.Annotations;

namespace playground
{
    [UsedImplicitly]
    [StateDescriptor(group = 3, priority = 5)]
    public class MeleeAttack : State
    {
        private int combo;

        public override void Init(Actor actor)
        {
            combo = 0;
        }

        public override void Update(Actor actor)
        {
            if (time >= .5f) Exit();
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