using CSM;
using JetBrains.Annotations;

namespace playground
{

    [UsedImplicitly]
    [StateDescriptor(group = 3, priority = 5)]
    [Require(typeof(Grounded))]
    public class Sprint : State
    {
        public float sprintSpeed = 8f;

        public override Stats? Update(Actor actor, Stats stats)
        {
            stats.speed = sprintSpeed;
            return stats;
        }

        public override bool Process(Actor actor, Message message)
        {
            if (message.name == "Sprint" && message.phase == Message.Phase.Ended) Exit();
            return false;
        }
        
    }
}