using UnityEngine;
namespace CSM.States
{
    [Solo]
    public class Dead : State
    {
        override public void Init(Actor actor)
        {
            Rigidbody rb = actor.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(actor.transform.forward * 10, ForceMode.Impulse);
            rb.AddForce(actor.transform.up * 10, ForceMode.Impulse);
        }

        override public void Update(Actor actor)
        {
        }

        override public void Process(Actor actor, Action action)
        {
            Next(actor, action);
        }

        override public void End(Actor actor)
        {
        }

    }
}