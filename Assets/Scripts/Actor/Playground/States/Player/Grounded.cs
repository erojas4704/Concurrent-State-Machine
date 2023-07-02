using UnityEngine;
using CSM;

namespace playground
{
    [StateDescriptor(priority = 3, group = 0)]
    public class Grounded : Movable
    {
        /** How accurately must a player be facing a ladder to be able to climb it.**/
        private float climbAngleMin = 0.85f;

        public override void Init(Actor actor)
        {
            base.Init(actor);
            controller = actor.GetComponent<CharacterController>();
            actor.velocity.y = -10f;
        }

        public override void Update(Actor actor)
        {
            base.Update(actor);
            if (!controller.isGrounded) Enter(typeof(Airborne));
        }

        public override void Process(Actor actor, Action action)
        {
            if (action.phase == Action.ActionPhase.Pressed)
            {
                switch (action.name)
                {
                    case "Jump":
                        action.processed = true;
                        actor.EnterState<Jump>();
                        break;
                    case "Sprint":
                        action.processed = true;
                        actor.EnterState<Sprint>();
                        break;
                    case "Ladder":
                        action.processed = true;
                        if (CanClimb(actor, action.GetInitiator<Ladder>()))
                        {
                            actor.EnterState<Climb>(action);
                        }
                        break;
                }
            }

            if (action.name == "Move")
            {
                axis = action.axis;
                action.processed = true;
            }
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