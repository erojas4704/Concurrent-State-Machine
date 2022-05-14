using CSM.Entities;
using UnityEngine;
using System;

namespace CSM.States
{
    [StateDescriptor(priority = 1)]
    public class InMotion : EntityState
    {
        private Player player;
        private CharacterController controller;
        private Vector2 axis;

        override public void Init(Entity entity)
        {
            player = entity.GetComponent<Player>();
            controller = entity.GetComponent<CharacterController>();
        }

        override public void Update(Entity entity)
        {
            axis = player.axis;
            entity.velocity.x = axis.x * stats.speed;
            entity.velocity.z = axis.y * stats.speed;
            controller.Move(entity.velocity * Time.deltaTime);
        }

    }
}