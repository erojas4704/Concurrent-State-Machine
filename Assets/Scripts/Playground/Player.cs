using CSM;
using Playground.States;
using Playground.States.Player;

namespace playground
{
    public class Player : PlayerActor
    {
        private Actor actor;

        private void Start()
        {
            actor = GetComponent<Actor>();
            actor.EnterState<Airborne>();
            actor.EnterState<MeleeArmed>();
            actor.stats = new PlayerStats();
        }
    }
}