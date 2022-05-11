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
            Group = 0;
            Priority = 1;
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
            if (action.name == "Jump")
            {
                actor.EnterState<Jump>();
            }

            if (action.name == "Move")
            {
                Vector2 axis = action.GetValue<Vector2>();
                this.axis = axis;
            }
            Next(actor, action);
        }

        override public void End(Actor actor)
        {
        }

    }
}