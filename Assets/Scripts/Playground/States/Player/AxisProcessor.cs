using CSM;
using JetBrains.Annotations;
using UnityEngine;

namespace Playground.States.Player
{
    [UsedImplicitly]
    [StateDescriptor(priority = 99, hidden = true)]
    public class AxisProcessor : State<PlayerStats>
    {
        public override bool Process(Message message)
        {
            if (message.name == "Move")
            {
                Vector2 axis;
                if (message.IsStartedOrHeld)
                {
                    axis = message.GetValue<Vector2>();
                }
                else
                {
                    axis = Vector2.zero;
                }

                stats.axis = axis;
                message.processed = true;
            }

            return false;
        }
    }
}