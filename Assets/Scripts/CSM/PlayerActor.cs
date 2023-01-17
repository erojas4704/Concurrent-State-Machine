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

        public void OnMove(InputAction.CallbackContext context)
        {
            axis = context.ReadValue<Vector2>();

            Action action = new()
            {
                name = context.action.name,
                phase = Action.TranslateToActionPhase(context.phase)
            };
            action.SetValue(context.ReadValue<Vector2>());
            action.axis = axis;
            PropagateAction(action, false);
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