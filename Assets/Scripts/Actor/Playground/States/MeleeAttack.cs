using CSM;
using JetBrains.Annotations;

namespace playground
{
    [UsedImplicitly]
    [StateDescriptor(group = 3, priority = 5)]
    public class MeleeAttack : State
    {
        private int combo;

        public override void Init(Actor actor)
        {
            combo = 0;
        }

        public override void Update(Actor actor)
        {
            if (time >= 2f) Exit(this.GetType());
        }

        public override bool Process(Actor actor, Action action)
        {
            if (action.phase == Action.ActionPhase.Pressed)
            {
                if (action.name == "Attack")
                {
                    action.processed = true;
                    combo++;
                }
            }

            return true;
        }
    }
}