using System;

namespace csm.entity
{
    [Serializable]
    public struct Stats
    {
        public float speed;
        public float acceleration;
        public float friction;

        public Stats(float speed, float acceleration, float friction)
        {
            this.speed = speed;
            this.acceleration = acceleration;
            this.friction = friction;
        }

        public override string ToString()
        {
            return $"Speed: {speed} ";
        }
    }
}