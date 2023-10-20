using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Serialization;

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
        public bool isBufferable;
        public float activationTime;

        /**If true, this message will be held*/
        public bool hold;

        public float Timer => Time.time - activationTime;

        public Message(string name)
        {
            this.name = name;
        }

        public Message(string name, Phase phase)
        {
            this.name = name;
            this.phase = phase;
        }

        public Message(string name, object trigger)
        {
            this.name = name;
            phase = Phase.Started;
            this.trigger = trigger;
        }

        public Message(string name, object trigger, Phase phase)
        {
            this.name = name;
            this.phase = phase;
            this.trigger = trigger;
        }

        public Message(InputAction.CallbackContext context)
        {
            InputAction action = context.action;
            name = action.name;
            phase = TranslateToActionPhase(context.phase);
            value = context.ReadValueAsObject();
        }

        public bool IsStartedOrHeld => phase is Phase.Started or Phase.Held;

        private object value;

        public void SetValue<T>(T newValue) where T : struct
        {
            value = newValue;
        }

        public T GetValue<T>() where T : struct
        {
            return (T)value;
        }

        private object trigger;

        public void SetTrigger<T>(T newTrigger)
        {
            trigger = newTrigger;
        }

        public T GetTrigger<T>()
        {
            return (T)trigger;
        }

        public static Phase TranslateToActionPhase(InputActionPhase phase)
        {
            return phase switch
            {
                InputActionPhase.Started => Phase.Started,
                InputActionPhase.Performed => Phase.Started,
                InputActionPhase.Canceled => Phase.Ended,
                _ => Phase.None
            };
        }

        public override string ToString()
        {
            string token = phase switch
            {
                Phase.Started => "🟢",
                Phase.Held => "🟡",
                Phase.Ended => "🔴",
                _ => "()"
            };

            return $"--> Action {name} {token}";
        }
    }
}