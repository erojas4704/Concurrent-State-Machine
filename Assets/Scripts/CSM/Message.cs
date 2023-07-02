using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace CSM
{
    [Serializable]
    public class Message
    {
        public string name;

        public enum Phase
        {
            None,
            Started,
            Held,
            Ended
        }

        public Phase phase = Phase.None;
        public bool processed;
        public float timer;
        public Vector2 axis;

        public Message(string name)
        {
            this.name = name;
        }

        public Message(string name, Phase phase)
        {
            this.name = name;
            this.phase = phase;
        }

        public Message(string name, object initiator)
        {
            this.name = name;
            phase = Phase.Started;
            _initiator = initiator;
        }

        public Message(string name, object initiator, Phase phase)
        {
            this.name = name;
            this.phase = phase;
            _initiator = initiator;
        }

        public Message(InputAction.CallbackContext context)
        {
            InputAction action = context.action;
            name = action.name;
            phase = TranslateToActionPhase(context.phase);
            //TODO This is a workaround to include an axis value. Remove this.
            if (context.valueType == typeof(Vector2))
            {
                axis = context.ReadValue<Vector2>();
            }
        }

        private object _value;

        public void SetValue<T>(T value) where T : struct
        {
            _value = value;
        }

        public T GetValue<T>() where T : struct
        {
            return (T)_value;
        }

        private object _initiator;

        public void SetInitiator<T>(T value)
        {
            _initiator = value;
        }

        public T GetInitiator<T>()
        {
            return (T)_initiator;
        }

        public static Phase TranslateToActionPhase(InputActionPhase phase)
        {
            return phase switch
            {
                InputActionPhase.Started => Phase.Started,
                InputActionPhase.Performed => Phase.Held,
                InputActionPhase.Canceled => Phase.Ended,
                _ => Phase.None
            };
        }

        public override string ToString()
        {
            string token = phase == Phase.Started ? "(v)"
                : phase == Phase.Held ? "(h)"
                : phase == Phase.Ended ? "(^)"
                : "()";

            return $"--> Action {name} {token}";
        }
    }
}