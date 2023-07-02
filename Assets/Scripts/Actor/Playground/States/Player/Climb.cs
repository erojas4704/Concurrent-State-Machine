using CSM;
using JetBrains.Annotations;
using UnityEngine;

namespace playground
{
    [UsedImplicitly]
    [StateDescriptor(group = 0, priority = 99)]
    public class Climb : State
    {
        private float climbSpeed = 5f;
        private PlayerActor player;
        private CharacterController controller;
        private Ladder ladder;

        public override void Init(CSM.Actor actor, Message inititator)
        {
            player = (PlayerActor)actor;
            controller = actor.GetComponent<CharacterController>();
            ladder = inititator.GetInitiator<Ladder>();

            controller.enabled = false;
        }

        public override void End(Actor actor)
        {
            controller.enabled = true;
        }

        public override Stats? Update(Actor actor, Stats stats)
        {
            Vector2 axis = player.axis;
            if (ladder == null) Exit();
            Vector3 ladderDirection = Vector3.Normalize(ladder.end.position - ladder.start.position);
            Vector3 futurePosition = player.transform.position +
                                     ladderDirection * climbSpeed * Time.deltaTime * player.axis.y;
            Vector3 nearestLadderPoint =
                FindNearestPointOnLadder(ladder.start.position, ladder.end.position, futurePosition);

            player.transform.position = nearestLadderPoint;

            //If i'm descending and my feet touch the ground, exit ladder state.
            if (axis.y < 0f && controller.isGrounded)
                actor.EnterState<Grounded>();
            
            return null;
        }

        public override bool Process(Actor actor, Message message)
        {
            if (message.phase == Message.Phase.Ended)
            {
                if (message.name == "Ladder")
                {
                    if (message.GetInitiator<Ladder>() == ladder)
                    {
                        //!!! Error-prone. Consider having a default state
                        //Enter(typeof(Airborne));
                    }
                }
            }

            return false;
        }

        private float GetProgressOnLadder(Vector3 origin, Vector3 end, Vector3 point)
        {
            float magnitudeMax = (end - origin).magnitude;

            Vector3 heading = Vector3.Normalize(end - origin);
            Vector3 lhs = point - origin;
            float dotP = Vector3.Dot(lhs, heading);

            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
            return dotP / magnitudeMax;
        }

        private Vector3 FindNearestPointOnLadder(Vector3 origin, Vector3 end, Vector3 point)
        {
            //https://stackoverflow.com/a/51906100
            float magnitudeMax = (end - origin).magnitude;

            Vector3 heading = Vector3.Normalize(end - origin);
            Vector3 lhs = point - origin;
            float dotP = Vector3.Dot(lhs, heading);

            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
            return origin + heading * dotP;
        }
    }
}