using UnityEngine;
using CSM.States;

namespace CSM.Entities.States
{
    [StateDescriptor(priority = 3, group = 0)]
    public class Grounded : Movable
    {

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
                        entity.EnterState<Climb>(action);
                        break;
                }
            }

            if (action.name == "Move")
            {
                action.processed = true;
            }
            Next(entity, action);
        }

    }
}