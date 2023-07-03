using System;
using CSM;

namespace Playground.States.Player
{    
    [Serializable]
    public record PlayerStats : Stats
    {
        public float sprintSpeed;
        public float acceleration;
        public float landAcceleration;
        public float airAcceleration;
        public float drag;
        public float ladderClimbSpeed;
        public float jumpHangTime;
    }
}