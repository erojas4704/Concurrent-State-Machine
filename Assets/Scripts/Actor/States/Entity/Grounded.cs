using UnityEngine;

namespace CSM.States
{
    [System.Serializable]

    public class Grounded : State
    {
        private CharacterController controller;
        private Entity entity;
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
            if (!controller.isGrounded)
            {
                actor.EnterState<Airborne>();
            }
        }

        override public void Process(Actor actor, Action action)
        {
            if (action.Name == "jump")
            {
                actor.EnterState<Jump>();
            }
            Next(actor, action);
        }

        override public void End(Actor actor)
        {
        }

    }
}