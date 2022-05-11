using UnityEngine.InputSystem;
using System;
namespace CSM
{
    public class Action
    {
        public string name;
        public enum ActionPhase { None, Pressed, Held, Released }
        public ActionPhase phase = ActionPhase.None;

        public Action() { }
        public Action(string name)
        {
            this.name = name;
        }

        public Action(InputAction action)
        {
            name = action.name;
            phase = TranslateToActionPhase(action.phase);
        }

        private Object _value;

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
    }
}