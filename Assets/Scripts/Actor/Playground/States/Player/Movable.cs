using System;
using UnityEngine;
using CSM;

namespace playground
{
    [StateDescriptor(priority = 1)]
    public abstract class Movable : State
    {
        private PlayerActor player;
        protected CharacterController controller;
        private Vector3 vel;

        public override void Init(Actor actor, Message initiator)
        {
            player = (PlayerActor)actor;
            controller = actor.GetComponent<CharacterController>();
        }

        public override Stats? Update(Actor actor, Stats stats)
        {
            //TODO demo: We shouldn't use the axis here.
            Vector3 targetVelocity = new()
            {
                x = player.axis.x * stats.speed,
                z = player.axis.y * stats.speed
            };


            //Use a flat version of our movement vector so the Y axis doesn't factor into the length calculation.
            Vector3 planarVelocity = actor.velocity;
            planarVelocity.y = 0;

            //Figure ot whether to use friction or acceleration
            float accelerationFactor = targetVelocity.magnitude > planarVelocity.magnitude
                ? stats.acceleration
                : stats.friction;

            //Every update, apply the acceleration * time to speed.
            float accelerationThisFrame = accelerationFactor * Time.deltaTime;
            actor.velocity.x = AccelerateWithClamping(accelerationThisFrame, actor.velocity.x, targetVelocity.x);
            actor.velocity.z = AccelerateWithClamping(accelerationThisFrame, actor.velocity.z, targetVelocity.z);

            controller.Move(actor.velocity * Time.deltaTime);
            return stats;
        }

        public override bool Process(Actor actor, Message message)
        {
            if (message.name == "Move")
            {
                player.axis = message.axis;
                message.processed = true;
            }
            return base.Process(actor, message);
        }

        private float AccelerateWithClamping(float accelerationThisFrame, float from, float to)
        {
            float sign = Mathf.Sign(to - from);
            float speedDelta = accelerationThisFrame * sign;
            if (Math.Abs(to - from) < 0.001f) return to;

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

        public override string ToString()
        {
            return base.ToString() + $" Axis: ({player.axis.x}, {player.axis.y})";
        }
    }
}