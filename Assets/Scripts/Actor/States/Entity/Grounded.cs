using UnityEngine;

namespace CSM.States
{
    [System.Serializable]

    public class Grounded : State
    {
        private Vector2 axis;
        private CharacterController controller;
        private Entity entity;

        public float speed = 5f;
        public Grounded()
        {
            group = 0;
            priority = 3;
        }

        override public void Init(Actor actor)
        {
            entity = actor.GetComponent<Entity>();
            entity.Velocity.y = -10f;
            controller = actor.GetComponent<CharacterController>();
        }

        override public void Update(Actor actor)
        {
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
                Debug.Log($"Got move action {action}");
                this.axis = action.axis;
                action.processed = true;
            }
            Next(actor, action);
        }

        override public void End(Actor actor)
        {
        }

    }
}