using CSM;
namespace CSM.States
{

    [StateDescriptor(group = 3, priority = 5)]
    [Require(typeof(Grounded))]
    public class Sprint : State
    {
        override public void Init(Actor actor)
        {
        }

        override public void Update(Actor actor)
        {
        }

        override public void Process(Actor actor, Action action)
        {
            if (action.name == "Sprint" && action.phase == Action.ActionPhase.Released) Exit(this.GetType());
            Next(actor, action);
        }

        override public void End(Actor actor)
        {
        }

    }
}