using CSM;
using JetBrains.Annotations;
using UnityEngine;

namespace Playground.States.Player
{
    [UsedImplicitly]
    [StateDescriptor(priority = 3, group = 0)]
    public class Airborne : Movable
    {

        public override void Update()
        {
            actor.velocity.y += -9.8f * Time.deltaTime;
            actor.velocity.y = Mathf.Max(actor.velocity.y, -50f); //Clamp to -50 terminal velocity
            if (controller.isGrounded && actor.velocity.y < 0f) actor.EnterState<Grounded>();

            stats.Acceleration = stats.AirAcceleration;
            stats.Friction = stats.Drag;
            base.Update();
        }

        public override bool Process(Message message)
        {
            if (message.name == "Attack" && message.phase == Message.Phase.Started)
            {
                message.processed = true;
                return true;
            }
            return base.Process(message);
        }
    }
}