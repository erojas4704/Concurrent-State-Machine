using UnityEngine;
using System;

namespace CSM.States
{
    [Serializable]

    public class Airborne : State
    {
        private Vector3 velocity;

        public Airborne()
        {
            Group = 0;
            Priority = 1;
        }

        override public void Update(Actor actor)
        {
            CharacterController controller = actor.GetComponent<CharacterController>();
            controller.Move(velocity * Time.deltaTime);
            velocity.y += -9.8f * Time.deltaTime;
            if (velocity.y < -10) velocity.y = -10;
            if (controller.isGrounded) Enter(typeof(Grounded));
        }

        override public void Process(Actor actor, Action action)
        {
            Next(actor, action);
        }
    }
}