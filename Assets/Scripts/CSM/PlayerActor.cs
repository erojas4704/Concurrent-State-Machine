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
            Message message = new(context);
            PropagateMessage(message);
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
                PropagateMessage(new Message(trigger.GetTriggerAction(), trigger), false);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            ITrigger trigger = other.GetComponent<ITrigger>();
            if (trigger != null)
            {
                PropagateMessage(new Message(trigger.GetTriggerAction(), trigger, Message.Phase.Ended),
                    false);
            }
        }
    }
}