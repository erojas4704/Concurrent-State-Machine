using UnityEngine;
using CSM;

namespace playground
{
    [StateDescriptor(priority = 3, group = 0)]
    public class Airborne : Movable
    {
        public float airAcceleration = 15f;
        public float drag = 3f;

        public override void Update(Actor actor)
        {
            base.Update(actor);
            CharacterController controller = actor.GetComponent<CharacterController>();
            actor.velocity.y += -9.8f * Time.deltaTime;
            actor.velocity.y = Mathf.Max(actor.velocity.y, -50f); //Clamp to -50 terminal velocity
            if (controller.isGrounded) actor.EnterState<Grounded>();
        }

        public override Stats Reduce(Actor actor, Stats stats)
        {
            stats.acceleration = airAcceleration;
            stats.friction = drag;
            return stats;
        }
    }
}