using System;

[Serializable]
public struct Stats
{
    public float speed;

    public Stats(float speed)
    {
        this.speed = speed;
    }

    public override string ToString()
    {
        return $"Speed: {speed} ";
    }
}