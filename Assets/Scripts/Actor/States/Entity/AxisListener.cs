using UnityEngine;
namespace CSM.States
{
    public class AxisListener : State
    {
        public Vector2 axis;
        public AxisListener()
        {
            Group = -2;
            Priority = 99;
        }

        override public void Init(Actor actor)
        {
        }

        override public void Update(Actor actor)
        {
            Action action = new Action("Move");
            action.SetValue(axis);
            Next(actor, action);
        }

        override public void Process(Actor actor, Action action)
        {
            if (action.name == "Move")
            {
                //State intercepts all axis input
                axis = action.GetValue<Vector2>();
                return;
            }
            Next(actor, action);
        }

        override public void End(Actor actor)
        {
        }

    }
}