using UnityEngine;

namespace Actor.Entity
{
    [System.Serializable]
    public class Airborne : State
    {
        private Vector3 velocity;

        override public void Update(Actor actor)
        {
            CharacterController controller = actor.GetComponent<CharacterController>();
            controller.Move(velocity * Time.deltaTime);
            velocity.y += -9.8f * Time.deltaTime;
            if (velocity.y < -10)
            {
                velocity.y = -10;
            }
        }

        override public void Process(Actor actor, Action action)
        {
            if (action.name == "jump")
            {
                velocity.y = 10f;
                Debug.Log("show me jumping");
            }

            Next(actor, action);
        }
    }
}