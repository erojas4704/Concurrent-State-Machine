using CSM;

namespace playground
{

    [StateDescriptor(group = 3, priority = 5)]
    [Require(typeof(Grounded))]
    public class Sprint : State
    {
        public float sprintSpeed = 8f;
        public override void Process(Actor actor, Action action)
        {
            if (action.name == "Sprint" && action.phase == Action.ActionPhase.Released) Exit(this.GetType());
            Next(actor, action);
        }

        public override Stats Reduce(Actor entity, Stats stats)
        {
            stats.speed = sprintSpeed;
            return stats;
        }
    }
}