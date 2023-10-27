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
            if (context.phase == InputActionPhase.Started) return; //We only listen to InputActionPhase.Performed

            Message message = new Message(context);
            if (context.action.type == InputActionType.Value)
                EnqueueMessage(message, false); //We don't want to propagate value messages, like analog sticks
            else
            {
                message.hold = true;
                EnqueueMessage(message);
            }
        }

        private void OnDisable()
        {
            actionMap.actionTriggered -= OnAction;
            actionMap.Disable();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            PropagateImmediate(new Message("ControllerCollision", hit, Message.Phase.Started));
        }

        public void OnTriggerEnter(Collider other)
        {
            ITrigger trigger = other.GetComponent<ITrigger>();
            if (trigger != null)
            {
                PropagateImmediate(new Message(trigger.GetTriggerAction(), trigger));
            }
        }

        public void OnTriggerExit(Collider other)
        {
            ITrigger trigger = other.GetComponent<ITrigger>();
            if (trigger != null)
            {
                PropagateImmediate(new Message(trigger.GetTriggerAction(), trigger, Message.Phase.Ended));
            }
        }
    }
}