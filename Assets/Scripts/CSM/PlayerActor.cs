using UnityEngine;
using UnityEngine.InputSystem;

namespace CSM
{
    public class PlayerActor : Actor
    {
        [HideInInspector] public Vector2 axis;
        public InputActionMap actionMap;

        private void OnAction(InputAction.CallbackContext context)
        {
            Action action = new(context);
            PropagateAction(action);
            if (context.action.name == "Move")
                axis = action.axis;
        }

        private void OnEnable()
        {
            actionMap.actionTriggered += OnAction;
            actionMap.Enable();
        }

        private void OnDisable()
        {
            actionMap.actionTriggered -= OnAction;
            actionMap.Disable();
        }

        public void OnTriggerEnter(Collider other)
        {
            ITrigger trigger = other.GetComponent<ITrigger>();
            if (trigger != null)
            {
                PropagateAction(new Action(trigger.GetTriggerAction(), trigger), false);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            ITrigger trigger = other.GetComponent<ITrigger>();
            if (trigger != null)
            {
                PropagateAction(new Action(trigger.GetTriggerAction(), trigger, Action.ActionPhase.Released),
                    false);
            }
        }
    }
}