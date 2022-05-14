using UnityEngine;

namespace CSM.States
{
    [System.Serializable]
    [StateDescriptor(priority = 3, group = 0)]
    public class Grounded : State
    {
        private Vector2 axis;
        private CharacterController controller;
        private Entity entity;
        private Player player;

        public float speed = 5f;
        
        override public void Init(Actor actor)
        {
            entity = actor.GetComponent<Entity>();
            controller = actor.GetComponent<CharacterController>();
            player = actor.GetComponent<Player>();
            entity.Velocity.y = -10f;
        }

        override public void Update(Actor actor)
        {
            axis = player.axis;
            if (!controller.isGrounded) actor.EnterState<Airborne>();
            entity.Velocity.x = axis.x * speed;
            entity.Velocity.z = axis.y * speed;
        }

        override public void Process(Actor actor, Action action)
        {
            if (action.phase == Action.ActionPhase.Pressed)
            {
                if (action.name == "Jump")
                {
                    action.processed = true;
                    actor.EnterState<Jump>();
                }
            }

            if (action.name == "Move")
            {
                action.processed = true;
            }
            Next(actor, action);
        }

        override public void End(Actor actor)
        {
        }

    }
}