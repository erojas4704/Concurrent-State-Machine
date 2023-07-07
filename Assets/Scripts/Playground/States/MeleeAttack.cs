using CSM;
using JetBrains.Annotations;
using Playground.States.Player;
using UnityEngine;

namespace Playground.States
{
    [UsedImplicitly]
    [StateDescriptor(group = 3, priority = 5)]
    public class MeleeAttack : State
    {
        private int combo;
        private Vector2 axis;
        private PlayerActor player;

        public override void Init(Actor actor, Message initiator)
        {
            player = (PlayerActor)actor;
            combo = 0;
        }

        public override Stats Update(Actor actor, Stats stats)
        {
            PlayerStats pStats = stats as PlayerStats;
            pStats.Speed *= 0.5f;
            if (time >= .5f) Exit();
            return stats;
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
            if (message.name == "Move")
            {
                message.processed = true;
                axis = message.axis;
            }
            return true; //Stop all states below from processing inputs.
        }

        public override void End(Actor actor)
        {
            player.axis = axis;
            base.End(actor);
        }
    }
}