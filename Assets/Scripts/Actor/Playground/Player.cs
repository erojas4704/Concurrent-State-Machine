using System;
using UnityEngine;
using UnityEngine.InputSystem;
using CSM;
using Action = CSM.Action;

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
        }

        public void OnAction(InputAction.CallbackContext context)
        {
            if (context.action.phase == InputActionPhase.Started)
            {
                Debug.Log($"There was an action or whatever {context.action.name}");
            }

            Action action = new Action(context);
            actor.PropagateAction(action);
            if (context.action.name == "Move")
                axis = action.axis;
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

        private void Update()
        {
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