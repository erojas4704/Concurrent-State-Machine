using CSM;
using JetBrains.Annotations;
using Playground.States.Player;
using UnityEngine;

namespace playground
{
    [UsedImplicitly]
    [StateDescriptor(group = 0, priority = 99)]
    public class Climb : State<PlayerStats>
    {
        private const float TOLERANCE = 0.98f;
        private float climbSpeed = 5f;
        private PlayerActor player;
        private CharacterController controller;
        private Ladder ladder;

        public override void Init(Message inititator)
        {
            player = (PlayerActor)actor;
            controller = actor.GetComponent<CharacterController>();
            ladder = inititator.GetInitiator<Ladder>();

            controller.enabled = false;
        }

        public override void End()
        {
            controller.enabled = true;
        }

        public override void Update()
        {
            Vector2 axis = player.axis;
            if (ladder == null) Exit();
            Vector3 ladderDirection = Vector3.Normalize(ladder.end.position - ladder.start.position);
            Vector3 futurePosition = player.transform.position +
                                     ladderDirection * (climbSpeed * Time.deltaTime * axis.y);
            Vector3 nearestLadderPoint =
                FindNearestPointOnLadder(ladder.start.position, ladder.end.position, futurePosition);

            player.transform.position = nearestLadderPoint;
            
            float progress = GetProgressOnLadder(ladder.start.position, ladder.end.position, actor.transform.position);
            if (progress > TOLERANCE)
            {
                //Snap to landing
                SnapActorToLanding();
            }
            

            //If i'm descending and my feet touch the ground, exit ladder state.
            if (axis.y < 0f && controller.isGrounded)
                actor.EnterState<Grounded>();
        }

        public override bool Process(Message message)
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

            if (message.name == "Move")
            {
                player.axis = message.axis;
                message.processed = true;
            }

            if (message.name == "Jump")
            {
                Exit();
                actor.EnterState<Jump>();
            }

            return false;
        }
        
        
        private void SnapActorToLanding()
        {
            Transform transform = actor.transform;
            Vector3 actorPosition = transform.position;
            Vector3 bottomPoint = actorPosition - new Vector3(0f, controller.height / 2f, 0f);
            Vector3 offset = ladder.landing.position - bottomPoint;

            Vector3 newPosition = actorPosition + offset;
            transform.position = newPosition;
            Exit();
            actor.EnterState<Airborne>(); //TODO <- Needs default state to avoid having to do this.
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