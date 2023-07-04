using System;
using CSM;
using Unity.VisualScripting;
using UnityEngine;

namespace Playground.States.Player
{
    [Serializable]
    public class PlayerStats : Stats
    {
        [Serialize] public float sprintSpeed;
        [HideInInspector] public float acceleration;
        public float landAcceleration;
        public float airAcceleration;
        public float drag;
        public float ladderClimbSpeed;
        public float jumpHangTime;

        public override Stats Clone()
        {
            return this;
        }
    }
}