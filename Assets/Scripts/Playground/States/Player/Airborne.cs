using CSM;
using JetBrains.Annotations;
using UnityEngine;

namespace Playground.States.Player
{
    [UsedImplicitly]
    [StateDescriptor(priority = 3, group = 0)]
    public class Airborne : Movable
    {

        public override void Update(Actor actor, Stats stats)
        {
            actor.velocity.y += -9.8f * Time.deltaTime;
            actor.velocity.y = Mathf.Max(actor.velocity.y, -50f); //Clamp to -50 terminal velocity
            if (controller.isGrounded) actor.EnterState<Grounded>();

            PlayerStats pStats = stats as PlayerStats;
            pStats!.Acceleration = pStats.AirAcceleration;
            pStats.Friction = pStats.Drag;
            base.Update(actor, stats);
        }
    }
}