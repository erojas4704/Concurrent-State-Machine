using CSM;
using Playground.States;
using UnityEngine;

namespace playground
{
    public class Player : MonoBehaviour
    {
        private Actor actor;

        private void Start()
        {
            actor = GetComponent<Actor>();
            actor.EnterState<Airborne>();
            actor.EnterState<MeleeArmed>();
        }
    }
}