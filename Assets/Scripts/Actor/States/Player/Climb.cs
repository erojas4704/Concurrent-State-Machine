using CSM.Entities;
using CSM.States;
using System.Collections;
using UnityEngine;

namespace CSM.Entities.States
{
    [StateDescriptor(group = 0, priority = 99)]
    public class Climb : EntityState
    {
        private float climbSpeed = 5f;
        private Player player;
        private CharacterController controller;
        public override void Init(Entity entity, Action inititator)
        {
            player = entity.GetComponent<Player>();
            controller = entity.GetComponent<CharacterController>();
        }
        public override void Update(Entity entity)
        {
            Vector2 axis = player.axis;
            controller.Move(new Vector3(0f, player.axis.y * climbSpeed * Time.deltaTime, 0f ));
        }

        public override void Process(Entity entity, Action action)
        {
            base.Process(entity, action);
            Debug.Log(action);
        }

    }
}