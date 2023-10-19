using System;
using CSM;
using Playground.States.Player;
using UnityEngine;

namespace Playground
{
    [RequireComponent(typeof(Actor))]
    public class StateEnterer : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Actor>().EnterState<MeleeArmed>();
        }
    }
}