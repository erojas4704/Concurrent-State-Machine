using System.Diagnostics;
using CSM;
using JetBrains.Annotations;
using Playground.States.Player;

namespace Playground.States
{

    [UsedImplicitly]
    [StateDescriptor(group = 3, priority = 5)]
    [Require(typeof(Grounded))]
    public class Sprint : State
    {
        public override void Update(Actor actor, Stats stats)
        {
            PlayerStats pStats = stats as PlayerStats;
            pStats.Speed = pStats.sprintSpeed;
        }

        public override bool Process(Actor actor, Message message)
        {
            if (message.name == "Sprint" && message.phase == Message.Phase.Ended) Exit();
            return false;
        }
        
    }
}