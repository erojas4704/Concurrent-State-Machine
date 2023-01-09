using UnityEngine;
using CSM;
using CSM.Entity;

namespace playground
{
    [StateDescriptor(priority = 3, group = 0)]
    public class Airborne : Movable
    {
        public float airAcceleration = 15f;
        public float drag = 3f;

        public override void Update(Entity entity)
        {
            base.Update(entity);
            CharacterController controller = entity.GetComponent<CharacterController>();
            entity.velocity.y += -9.8f * Time.deltaTime;
            entity.velocity.y = Mathf.Max(entity.velocity.y, -50f); //Clamp to -50 terminal velocity
            if (controller.isGrounded) Enter(typeof(Grounded));
        }

        public override Stats Reduce(Entity entity, Stats stats)
        {
            stats.acceleration = airAcceleration;
            stats.friction = drag;
            return stats;
        }
    }
}