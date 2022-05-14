using UnityEngine;
using System;
using CSM;
using CSM.States;

namespace CSM.Entities.States
{
    [StateDescriptor(priority = 3, group = 0)]
    public class Airborne : EntityState
    {

        override public void Update(Entity entity)
        {
            CharacterController controller = entity.GetComponent<CharacterController>();
            entity.velocity.y += -9.8f * Time.deltaTime;
            if (entity.velocity.y < -10) entity.velocity.y = -10;
            if (controller.isGrounded) Enter(typeof(Grounded));
        }
    }
}