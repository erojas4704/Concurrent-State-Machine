using UnityEngine;
using CSM.Entities;

namespace CSM.States
{
    [StateDescriptor(priority = 3, group = 0)]
    public class Grounded : EntityState
    {
        private Vector2 axis;
        private CharacterController controller;
        private Player player;

        override public void Init(Entity entity)
        {
            controller = entity.GetComponent<CharacterController>();
            player = entity.GetComponent<Player>();
            entity.velocity.y = -10f;
        }

        override public void Update(Entity entity)
        {
            axis = player.axis;
            if (!controller.isGrounded) Enter(typeof(Airborne));
            entity.velocity.x = axis.x * stats.speed;
            entity.velocity.z = axis.y * stats.speed;
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