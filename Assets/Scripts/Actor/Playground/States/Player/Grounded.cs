using UnityEngine;
using CSM;

namespace playground
{
    [StateDescriptor(priority = 3, group = 0)]
    public class Grounded : Movable
    {
        /** How accurately must a player be facing a ladder to be able to climb it.**/
        private float climbAngleMin = 0.85f;

        public override void Init(Actor actor, Message initiator)
        {
            base.Init(actor, initiator);
            controller = actor.GetComponent<CharacterController>();
            actor.velocity.y = -10f;
        }

        public override Stats? Update(Actor actor, Stats stats)
        {
            base.Update(actor, stats);
            if (!controller.isGrounded) actor.EnterState<Airborne>();
            return null;
        }

        public override bool Process(Actor actor, Message message)
        {
            if (message.phase == Message.Phase.Started)
            {
                switch (message.name)
                {
                    case "Jump":
                        message.processed = true;
                        actor.EnterState<Jump>();
                        break;
                    case "Sprint":
                        message.processed = true;
                        actor.EnterState<Sprint>();
                        break;
                    case "Ladder":
                        message.processed = true;
                        if (CanClimb(actor, message.GetInitiator<Ladder>()))
                        {
                            actor.EnterState<Climb>(message);
                        }

                        break;
                }
            }

            if (message.name == "Move")
            {
                axis = message.axis;
                message.processed = true;
            }

            return false;
        }

        private bool CanClimb(Actor actor, Ladder ladder)
        {
            Vector3 flatten = new Vector3(1f, 0f, 1f);
            Vector3 ladderFace = Vector3.Scale(ladder.transform.forward, flatten).normalized;
            Vector3 actorHeading = Vector3.Scale(actor.velocity, flatten).normalized;
            float dot = Vector3.Dot(ladderFace, actorHeading);
            if (dot > climbAngleMin)
                return true;

            return false;
        }
    }
}