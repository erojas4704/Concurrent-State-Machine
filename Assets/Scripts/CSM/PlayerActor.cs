using UnityEngine;
using UnityEngine.InputSystem;

namespace CSM
{
    public class PlayerActor : Actor
    {
        private InputActionMap actionMap;
        [SerializeField] private InputActionAsset actionAsset;

        private void OnEnable()
        {
            actionMap = actionAsset.FindActionMap("Player");
            actionMap.actionTriggered += OnAction;
            actionMap.Enable();
        }

        private void OnAction(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started) return;

            // if (context.action.name == "Attack")
            // {
            //     Debug.Log(context.phase);
            // }

            Message message = new Message(context);
            if (context.action.type == InputActionType.Value)
                PropagateMessage(message, false); //We don't want to propagate value messages, like analog sticks
            else
                PropagateMessage(message);
        }

        private void OnDisable()
        {
            actionMap.actionTriggered -= OnAction;
            actionMap.Disable();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            PropagateMessage(new("ControllerCollision", hit, Message.Phase.Started), false);
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