using System;

[Serializable]
public struct Stats
{
    public float speed;
    public float acceleration;

    public Stats(float speed, float acceleration)
    {
        this.speed = speed;
        this.acceleration = acceleration;
    }

    public override string ToString()
    {
        return $"Speed: {speed} ";
    }
}