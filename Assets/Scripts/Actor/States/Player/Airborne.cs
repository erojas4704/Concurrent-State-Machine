using UnityEngine;
using System;
using CSM.Entities;

namespace CSM.States
{
    [StateDescriptor(priority = 3, group = 0)]
    public class Airborne : State
    {
        private Entity entity;

        override public void Init(Actor actor)
        {
            entity = (Entity)actor;
        }

        override public void Update(Actor actor)
        {
            CharacterController controller = actor.GetComponent<CharacterController>();
            controller.Move(entity.velocity * Time.deltaTime);
            entity.velocity.y += -9.8f * Time.deltaTime;
            if (entity.velocity.y < -10) entity.velocity.y = -10;
            if (controller.isGrounded) Enter(typeof(Grounded));
        }

        override public void Process(Actor actor, Action action)
        {
            Next(actor, action);
        }
    }
}