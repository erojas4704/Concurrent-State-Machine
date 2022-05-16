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
        private Ladder ladder;

        public override void Init(Entity entity, Action inititator)
        {
            player = entity.GetComponent<Player>();
            controller = entity.GetComponent<CharacterController>();

            ladder = inititator.GetInitiator<Ladder>();
        }
        public override void Update(Entity entity)
        {
            Vector2 axis = player.axis;
            if (ladder == null) Exit(this.GetType());
            Vector3 ladderDirection = Vector3.Normalize(ladder.end.position - ladder.start.position);
            //controller.Move(new Vector3(0f, player.axis.y * climbSpeed * Time.deltaTime, 0f));
            Vector3 futurePosition = player.transform.position + ladderDirection * climbSpeed * Time.deltaTime * player.axis.y;
            Vector3 nearestLadderPoint = FindNearestPointOnLadder(ladder.start.position, ladder.end.position, futurePosition);

            controller.Move(nearestLadderPoint - player.transform.position);

            //If i'm descending and my feet touch the ground, exit ladder state.
            if (axis.y < 0f && controller.isGrounded)
                Enter(typeof(Grounded));
        }

        public override void Process(Entity entity, Action action)
        {
            if (action.phase == Action.ActionPhase.Released)
            {
                if (action.name == "Ladder")
                {
                    if (action.GetInitiator<Ladder>() == ladder)
                    {
                        //!!! Error prone. Consider having a default state
                        Enter(typeof(Airborne));
                    }
                }
            }
            Next(entity, action);
        }

        private Vector3 FindNearestPointOnLadder(Vector3 origin, Vector3 end, Vector3 point)
        {
            Vector3 heading = Vector3.Normalize(end - origin);
            //float magnitudeMax = heading.magnitude;

            Vector3 lhs = point - origin;
            float dotP = Vector3.Dot(lhs, heading);
            return origin + heading * dotP;
        }

    }
}