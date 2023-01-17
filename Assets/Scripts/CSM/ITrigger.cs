namespace CSM
{
    /**An interface to be used by Collidable objects. When a trigger implementing this interface collides
     * with an Actor, a "Collision" Action will propagate through the Actor's states.
     */
    public interface ITrigger
    {
        public string GetTriggerAction();
    }
}