using UnityEngine;
using UnityEngine.InputSystem;
using CSM;

namespace playground
{
    [RequireComponent(typeof(Actor))]
    public class Player : MonoBehaviour
    {
        // [HideInInspector]
        public Vector2 axis;
        private Actor actor;
        public InputActionMap actionMap;

        void Start()
        {
            actor = GetComponent<Actor>();
            actor.EnterState<Airborne>();
            actor.EnterState<MeleeArmed>();
            actionMap.Enable();
            actionMap.actionTriggered += OnAction;
        }

        void Update()
        {

        }

        public void OnAction(InputAction.CallbackContext context)
        {
            Action action = new Action(context.action);
            actor.PropagateAction(action);
            action.axis = axis;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            axis = context.ReadValue<Vector2>();

            Action action = new Action
            {
                name = context.action.name,
                phase = Action.TranslateToActionPhase(context.phase)
            };
            action.SetValue(context.ReadValue<Vector2>());
            action.axis = axis;
            actor.PropagateAction(action, false);
        }

        public void OnEnable()
        {
            actionMap.Enable();
            actionMap.actionTriggered += OnAction;
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
                actor.PropagateAction(new Action("Ladder", other.GetComponent<Ladder>(), Action.ActionPhase.Released), false);
            }
        }
    }
}