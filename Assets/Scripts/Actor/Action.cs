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
        public bool processed = false;
        public float timer = 0f;
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

        public static ActionPhase TranslateToActionPhase(InputActionPhase phase)
        {
            switch (phase)
            {
                case InputActionPhase.Started:
                    return Action.ActionPhase.Pressed;
                case InputActionPhase.Performed:
                    return Action.ActionPhase.Held;
                case InputActionPhase.Canceled:
                    return Action.ActionPhase.Released;
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

    public class Action<T> : Action {
        private T initiator;
        
        public Action(string name, T initiator, ActionPhase phase)
        {
            this.initiator = initiator;
            this.name = name;
            this.phase = phase;
        }

        public T GetInitiator()
        {
            return initiator;
        }

        public void SetInitiator(T initiator)
        {
            this.initiator = initiator;
        }
    }
}