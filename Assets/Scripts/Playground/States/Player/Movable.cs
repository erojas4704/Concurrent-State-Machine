using System;
using CSM;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Playground.States.Player
{
    [StateDescriptor(priority = 1)]
    public abstract class Movable : State<PlayerStats>
    {
        private PlayerActor player;
        protected CharacterController controller;
        private Vector3 vel;
        private const float DIRECTION_CHANGE_THRESHOLD = 0.99f;

        public override void Init(Message initiator)
        {
            player = (PlayerActor)actor;
            controller = actor.GetComponent<CharacterController>();
        }

        public override void Update()
        {
            //TODO demo: We shouldn't use the axis here.

            Vector3 targetVelocity = new()
            {
                x = player.axis.x * stats.Speed,
                z = player.axis.y * stats.Speed
            };


            //Target Velocity will be 5 if we are airborne from a Sprint. Our base velocity will be 8.
            //in this case we should ignore target velocity and use drag.

            //Use a flat version of our movement vector so the Y axis doesn't factor into the length calculation.
            Vector3 planarVelocity = actor.velocity;
            planarVelocity.y = 0;

            float directionDifferenceFactor = Vector3.Dot(planarVelocity.normalized, targetVelocity.normalized);
            float accelerationFactor;

            //TODO this implementation isn't great.
            //Figure to whether to use friction or acceleration, based on whichever is greater.
            //If the target velocity is greater than current velocity, we will use acceleration.
            //Otherwise, we will use friction to decelerate.W

            //The issue lies in situations where we need to turn around and our speed is temporarily increased by an action, such as sprinting.
            //The max speed will be set to 5, but our own speed is greater than that. If we want to turn completely around and go 5 in the other
            //direction, it will use friction, because 5 is less than the applied sprinting speed of 8. 
            //We need to incorporate the difference in axis somehow. 

            if (directionDifferenceFactor >= DIRECTION_CHANGE_THRESHOLD)
            {
                accelerationFactor = targetVelocity.magnitude > planarVelocity.magnitude
                    ? stats.Acceleration
                    : stats.Friction;
            }
            else
            {
                accelerationFactor = stats.Acceleration;
            }

            if (planarVelocity.magnitude > 0.2f)
            {
                Quaternion headingDirection = Quaternion.LookRotation(planarVelocity);
                Quaternion currentRotation = actor.transform.rotation;
                Quaternion newRotation =
                    Quaternion.Slerp(currentRotation, headingDirection, stats.TurnSpeed * Time.deltaTime);
                actor.transform.rotation = newRotation;
            }

            //Every update, apply the acceleration * time to speed.
            float accelerationThisFrame = accelerationFactor * 0.5f * Time.deltaTime;
            actor.velocity.x = AccelerateWithClamping(accelerationThisFrame, actor.velocity.x, targetVelocity.x);
            actor.velocity.z = AccelerateWithClamping(accelerationThisFrame, actor.velocity.z, targetVelocity.z);

            controller.Move(actor.velocity * Time.deltaTime);

            actor.velocity.x = AccelerateWithClamping(accelerationThisFrame, actor.velocity.x, targetVelocity.x);
            actor.velocity.z = AccelerateWithClamping(accelerationThisFrame, actor.velocity.z, targetVelocity.z);
        }

        public override bool Process(Message message)
        {
            if (message.name == "Move")
            {
                player.axis = message.axis;
                message.processed = true;
            }

            return base.Process(message);
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