using UnityEngine;
using CSM;

namespace playground
{
    [Solo]
    public class Dead : State
    {
        public override void Init(Actor actor, Message initiator)
        {
            Rigidbody rb = actor.GetComponent<Rigidbody>();
            if (rb == null) rb = actor.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(actor.transform.forward * 10, ForceMode.Impulse);
            rb.AddForce(actor.transform.up * 10, ForceMode.Impulse);
        }
    }
}