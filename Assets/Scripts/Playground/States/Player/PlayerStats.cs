using System;
using CSM;
using Unity.VisualScripting;
using UnityEngine;

namespace Playground.States.Player
{
    [Serializable]
    public partial class PlayerStats : Stats
    {
        [Serialize] public float sprintSpeed;
        [HideInInspector] public float acceleration;
        private float hp;
        public float speed;
        public float friction;
        public float landAcceleration;
        public float airAcceleration;
        public float drag;
        public float ladderClimbSpeed;
        public float jumpHangTime;
    }
}