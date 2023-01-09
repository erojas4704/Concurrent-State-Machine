using CSM.Entities;
using UnityEngine;
using System;

namespace CSM.States
{
    [StateDescriptor(priority = 1)]
    public abstract class Movable : EntityState
    {
        private Player player;
        public CharacterController controller;
        public Vector2 axis;

        [SerializeField]
        private Vector3 vel;

        public override void Init(Entity entity)
        {
            player = entity.GetComponent<Player>();
            controller = entity.GetComponent<CharacterController>();
        }

        public override void Update(Entity entity)
        {
            axis = player.axis;
            Vector3 targetVelocity = new Vector3();
            targetVelocity.x = axis.x * stats.speed;
            targetVelocity.z = axis.y * stats.speed;

            //Use a flat version of our movement vector so the Y axis doesn't factor into the length calculation.
            Vector3 planarVelocity = entity.velocity;
            planarVelocity.y = 0;

            //Figure ot whether to use fiction or acceleration
            float accelerationFactor = targetVelocity.magnitude > planarVelocity.magnitude ?
                stats.acceleration : stats.friction;

            //Every update, apply the acceleration * time to speed.
            float accelerationThisFrame = accelerationFactor * Time.deltaTime;
            entity.velocity.x = AccelerateWithClamping(accelerationThisFrame, entity.velocity.x, targetVelocity.x);
            entity.velocity.z = AccelerateWithClamping(accelerationThisFrame, entity.velocity.z, targetVelocity.z);

            controller.Move(entity.velocity * Time.deltaTime);
        }

        private float AccelerateWithClamping(float accelerationThisFrame, float from, float to)
        {
            float sign = Mathf.Sign(to - from);
            float speedDelta = accelerationThisFrame * sign;
            if (to == from) return to;

            if (to > from)
            {
                if (from + speedDelta > to)
                    return to;
            }
            else if (to < from)
            {
                if (from + speedDelta < to)
                    return to;
            }
            /*
            if(Mathf.Abs(from) + speedDelta > Mathf.Abs(to))
                return to;
            */

            return from + speedDelta;
        }
    }
}