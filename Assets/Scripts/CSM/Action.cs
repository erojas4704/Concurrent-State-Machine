using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace CSM
{
    [Serializable]
    public class Action
    {
        public string name;
        public enum ActionPhase { None, Pressed, Held, Released }
        public ActionPhase phase = ActionPhase.None;
        public bool processed;
        public float timer;
        public Vector2 axis;

        public Action() { }
        public Action(string name)
        {
            this.name = name;
        }

        public Action(string name, ActionPhase phase)
        {
            this.name = name;
            this.phase = phase;
        }

        public Action(string name, System.Object initiator)
        {
            this.name = name;
            this.phase = ActionPhase.Pressed;
            this._initiator = initiator;
        }
        public Action(string name, System.Object initiator, ActionPhase phase)
        {
            this.name = name;
            this.phase = phase;
            this._initiator = initiator;
        }

        public Action(InputAction action)
        {
            name = action.name;
            phase = TranslateToActionPhase(action.phase);
        }

        private System.Object _value;

        public void SetValue<T>(T value) where T : struct
        {
            this._value = value;
        }
        public T GetValue<T>() where T : struct
        {
            return (T)this._value;
        }

        private System.Object _initiator;

        public void SetInitiator<T>(T value)
        {
            this._initiator = value;
        }
        public T GetInitiator<T>()
        {
            return (T)this._initiator;
        }

        public static ActionPhase TranslateToActionPhase(InputActionPhase phase)
        {
            switch (phase)
            {
                case InputActionPhase.Started:
                    return ActionPhase.Pressed;
                case InputActionPhase.Performed:
                    return ActionPhase.Held;
                case InputActionPhase.Canceled:
                    return ActionPhase.Released;
            }

            return ActionPhase.None;
        }

        public override string ToString()
        {
            String token = phase == ActionPhase.Pressed ? "(v)"
                : phase == ActionPhase.Held ? "(h)"
                : phase == ActionPhase.Released ? "(^)"
                : "()";

            return $"--> Action {name} {token}";
        }
    }
}