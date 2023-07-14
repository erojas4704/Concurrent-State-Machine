using CSM;
using JetBrains.Annotations;

namespace Playground.States.Player
{

    [UsedImplicitly]
    [StateDescriptor(group = 3, priority = 5)]
    [Require(typeof(Grounded))]
    public class Sprint : State<Playground.PlayerStats>
    {
        public override void Update()
        {
            stats.Speed = stats.sprintSpeed;
        }

        public override bool Process(Message message)
        {
            if (message.name == "Sprint" && message.phase == Message.Phase.Ended) Exit();
            return false;
        }
        
    }
}