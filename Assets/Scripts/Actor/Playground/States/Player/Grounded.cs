using UnityEngine;
using CSM;
using CSM.Entity;

namespace playground
{
    [StateDescriptor(priority = 3, group = 0)]
    public class Grounded : Movable
    {
        /** How accurately must a player be facing a ladder to be able to climb it.**/
        private float climbAngleMin = 0.85f;

        public override void Init(Entity entity)
        {
            base.Init(entity);
            controller = entity.GetComponent<CharacterController>();
            entity.velocity.y = -10f;
        }

        public override void Update(Entity entity)
        {
            base.Update(entity);
            if (!controller.isGrounded) Enter(typeof(Airborne));
        }

        public override void Process(Entity entity, Action action)
        {
            if (action.phase == Action.ActionPhase.Pressed)
            {
                switch (action.name)
                {
                    case "Jump":
                        action.processed = true;
                        entity.EnterState<Jump>();
                        break;
                    case "Sprint":
                        action.processed = true;
                        entity.EnterState<Sprint>();
                        break;
                    case "Ladder":
                        action.processed = true;
                        if (CanClimb(entity, action.GetInitiator<Ladder>()))
                        {
                            entity.EnterState<Climb>(action);
                        }
                        break;
                }
            }

            if (action.name == "Move")
            {
                action.processed = true;
            }
            Next(entity, action);
        }

        private bool CanClimb(Entity entity, Ladder ladder)
        {
            Vector3 flatten = new Vector3(1f, 0f, 1f);
            Vector3 ladderFace = Vector3.Scale(ladder.transform.forward, flatten).normalized;
            Vector3 entityHeading = Vector3.Scale(entity.velocity, flatten).normalized;
            float dot = Vector3.Dot(ladderFace, entityHeading);
            if (dot > climbAngleMin) 
                return true;

            return false;
        }

    }
}