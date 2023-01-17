using CSM;
using UnityEngine;
using Action = CSM.Action;

namespace playground
{
    public class Player : MonoBehaviour
    {
        private Actor actor;
        private void Start()
        {
            actor = GetComponent<Actor>();            
            actor.EnterState<Airborne>();
            actor.EnterState<MeleeArmed>();

        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Ladder>() != null)
            {
                //TODO, forward triggers to states and let them handle them by name
                //Try to reduce as much logic here as possible
                actor.PropagateAction(new Action("Ladder", other.GetComponent<Ladder>()), false);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<Ladder>() != null)
            {
                actor.PropagateAction(new Action("Ladder", other.GetComponent<Ladder>(), Action.ActionPhase.Released),
                    false);
            }
        }
    }
}