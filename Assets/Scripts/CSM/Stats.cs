using System;

namespace CSM
{
    [Serializable]
    public record Stats
    {
        public float speed;
        public float friction;

        public override string ToString()
        {
            return $"Speed: {speed} ";
        }
    }
}