using csm;
using csm.entity;

namespace playground
{
    [StateDescriptor(group = 3, priority = 5)]
    public class MeleeAttack : EntityState
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

        public override void Process(Actor actor, Action action)
        {
            if (action.phase == Action.ActionPhase.Pressed)
            {
                if (action.name == "Attack")
                {
                    action.processed = true;
                    combo++;
                    return; //Eat the input so it doesn't trigger the next state
                }
            }

            //Next(actor, action);
        }
    }
}