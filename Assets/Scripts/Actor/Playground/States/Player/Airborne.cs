using UnityEngine;
using CSM;
using JetBrains.Annotations;

namespace playground
{
    [UsedImplicitly]
    [StateDescriptor(priority = 3, group = 0)]
    public class Airborne : Movable
    {
        private const float AIR_ACCELERATION = 15f;
        private const float DRAG = 3f;

        public override Stats? Update(Actor actor, Stats stats)
        {
            actor.velocity.y += -9.8f * Time.deltaTime;
            actor.velocity.y = Mathf.Max(actor.velocity.y, -50f); //Clamp to -50 terminal velocity
            if (controller.isGrounded) actor.EnterState<Grounded>();
            
            stats.acceleration = AIR_ACCELERATION;
            stats.friction = DRAG;
            return base.Update(actor, stats);
        }
    }
}