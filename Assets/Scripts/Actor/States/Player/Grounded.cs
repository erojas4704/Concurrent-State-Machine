using UnityEngine;
using CSM.States;

namespace CSM.Entities.States
{
    [StateDescriptor(priority = 3, group = 0)]
    public class Grounded : EntityState
    {
        private CharacterController controller;

        override public void Init(Entity entity)
        {
            controller = entity.GetComponent<CharacterController>();
            entity.velocity.y = -10f;
        }

        override public void Update(Entity entity)
        {
            if (!controller.isGrounded) Enter(typeof(Airborne));
        }

        override public void Process(Entity entity, Action action)
        {
            if (action.phase == Action.ActionPhase.Pressed)
            {
                if (action.name == "Jump")
                {
                    action.processed = true;
                    entity.EnterState<Jump>();
                }

                if (action.name == "Sprint")
                {
                    action.processed = true;
                    entity.EnterState<Sprint>();
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