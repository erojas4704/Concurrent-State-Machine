using CSM.States;
namespace CSM.Entities.States
{

    [StateDescriptor(group = 3, priority = 5)]
    [Require(typeof(Grounded))]
    public class Sprint : EntityState
    {
        public float sprintSpeed = 8f;
        public override void Process(Entity actor, Action action)
        {
            if (action.name == "Sprint" && action.phase == Action.ActionPhase.Released) Exit(this.GetType());
            Next(actor, action);
        }

        public override Stats Reduce(Entity entity, Stats stats)
        {
            stats.speed = sprintSpeed;
            return stats;
        }
    }
}