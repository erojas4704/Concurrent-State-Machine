using UnityEngine;
using System;

namespace CSM.States
{
    [Serializable]
    [StateDescriptor(priority = 3, group = 0)]
    public class Airborne : State
    {
        private Entity entity;

        override public void Init(Actor actor)
        {
            entity = actor.GetComponent<Entity>();
        }

        override public void Update(Actor actor)
        {
            CharacterController controller = actor.GetComponent<CharacterController>();
            controller.Move(entity.Velocity * Time.deltaTime);
            entity.Velocity.y += -9.8f * Time.deltaTime;
            if (entity.Velocity.y < -10) entity.Velocity.y = -10;
            if (controller.isGrounded) Enter(typeof(Grounded));
        }

        override public void Process(Actor actor, Action action)
        {
            Next(actor, action);
        }
    }
}