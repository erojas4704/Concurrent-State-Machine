using System;

[Serializable]
public struct Stats
{
    public float speed;
    public float jumpForce;

    public Stats(float speed, float jumpForce)
    {
        this.speed = speed;
        this.jumpForce = jumpForce;
    }

    public override string ToString()
    {
        return $"Speed: {speed} JumpForce: {jumpForce}";
    }
}