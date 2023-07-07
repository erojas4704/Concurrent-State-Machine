using CSM;
using playground;
using UnityEngine;

namespace Playground.States.Player
{
    [StateDescriptor(priority = 3, group = 0)]
    public class Grounded : Movable
    {
        /** How accurately must a player be facing a ladder to be able to climb it.**/
        private const float CLIMB_ANGLE_MIN = 0.85f;

        public override void Init(Message initiator)
        {
            base.Init(initiator);
            controller = actor.GetComponent<CharacterController>();
            actor.velocity.y = -10f;
        }

        public override void Update()
        {
            if (!controller.isGrounded)
            {
                actor.Persist(this, stats.CoyoteTime);
                actor.EnterState<Airborne>();
            }
            base.Update();
        }

        public override bool Process(Message message)
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
            
            return base.Process(message);
        }

        private bool CanClimb(Actor actor, Ladder ladder)
        {
            Vector3 flatten = new Vector3(1f, 0f, 1f);
            Vector3 ladderFace = Vector3.Scale(ladder.transform.forward, flatten).normalized;
            Vector3 actorHeading = Vector3.Scale(actor.velocity, flatten).normalized;
            float dot = Vector3.Dot(ladderFace, actorHeading);
            if (dot > CLIMB_ANGLE_MIN)
                return true;

            return false;
        }
    }
}