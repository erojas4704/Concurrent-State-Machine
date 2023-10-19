using CSM;
using JetBrains.Annotations;
using UnityEngine;

namespace Playground.States.Player
{
    [UsedImplicitly]
    [StateDescriptor(priority = 2, group = 4)]
    public class MeleeArmed : State
    {
        public override bool Process(Message message)
        {
            if (message.name == "Attack" && message.phase == Message.Phase.Held)
            {
                Debug.Log("ATTACK HELD");
                return false;
            }

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