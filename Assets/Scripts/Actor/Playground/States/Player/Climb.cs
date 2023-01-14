using CSM;
using UnityEngine;

namespace playground
{
    [StateDescriptor(group = 0, priority = 99)]
    public class Climb : State
    {
        private float climbSpeed = 5f;
        private Player player;
        private CharacterController controller;
        private Ladder ladder;

        public override void Init(Actor actor, Action inititator)
        {
            player = actor.GetComponent<Player>();
            controller = actor.GetComponent<CharacterController>();
            ladder = inititator.GetInitiator<Ladder>();

            controller.enabled = false;
        }

        public override void End(Actor actor)
        {
            controller.enabled = true;
        }

        public override void Update(Actor actor)
        {
            Vector2 axis = player.axis;
            if (ladder == null) Exit(this.GetType());
            Vector3 ladderDirection = Vector3.Normalize(ladder.end.position - ladder.start.position);
            Vector3 futurePosition = player.transform.position + ladderDirection * climbSpeed * Time.deltaTime * player.axis.y;
            Vector3 nearestLadderPoint = FindNearestPointOnLadder(ladder.start.position, ladder.end.position, futurePosition);

            player.transform.position = nearestLadderPoint;

            //If i'm descending and my feet touch the ground, exit ladder state.
            if (axis.y < 0f && controller.isGrounded)
                Enter(typeof(Grounded));
        }

        public override void Process(Actor actor, Action action)
        {
            if (action.phase == Action.ActionPhase.Released)
            {
                if (action.name == "Ladder")
                {
                    if (action.GetInitiator<Ladder>() == ladder)
                    {
                        //!!! Error-prone. Consider having a default state
                        //Enter(typeof(Airborne));
                    }
                }
            }
            Next(actor, action);
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