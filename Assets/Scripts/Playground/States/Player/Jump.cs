using CSM;
using JetBrains.Annotations;
using Playground.States.Player;

namespace playground
{
    [UsedImplicitly]
    [StateDescriptor(priority = 2, group = 2)]
    [With(typeof(Airborne))]
    public class Jump : State<PlayerStats>
    {
        private bool isHeld;

        public override void Init(Message initiator)
        {
            actor.velocity.y = stats.JumpForce;
        }

        public override void Update()
        {
            if (actor.velocity.y < 0)
            {
                if (isHeld && time < stats.JumpHangTime)
                    actor.velocity.y = 0f;
                else
                    Exit();
            }
        }

        public override bool Process(Message message)
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