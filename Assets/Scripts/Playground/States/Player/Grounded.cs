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

        private float lastSurfaceHeight;

        public override void Init(Message initiator)
        {
            base.Init(initiator);
            controller = actor.GetComponent<CharacterController>();
            actor.velocity.y = -1f;
        }

        public override void Update()
        {
            if (!controller.isGrounded)
            {
                actor.Persist(this, stats.CoyoteTime);
                actor.EnterState<Airborne>();
                lastSurfaceHeight = actor.transform.position.y;
            }

            base.Update();
        }

        public override bool Process(Message message)
        {
            if (message.phase == Message.Phase.Started)
            {
                switch (message.name)
                {
                    //TODO Consume("Jump", () => {}) pattern ?
                    case "Jump":
                        message.processed = true;
                        //if ghost, we will reset the actor's Y position to the last position they were at.
                        //actor.transform.position.y = lastSurfaceHeight; //Obviously circumvent the character controller or work with it.
                        if (IsGhost)
                        {
                            Vector3 position = actor.transform.position;
                            Vector3 moveDelta = new Vector3(position.x,
                                lastSurfaceHeight, position.z) - position;
                            controller.Move(moveDelta);
                        }

                        actor.EnterState<Jump>();
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

            if (message.phase is Message.Phase.Held or Message.Phase.Started)
            {
                switch (message.name)
                {
                    case "Sprint":
                        message.processed = true;
                        actor.EnterState<Sprint>();
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