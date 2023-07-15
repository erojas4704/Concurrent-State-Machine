using CSM;
using JetBrains.Annotations;
using UnityEngine;

namespace Playground.States.Common
{
    [Solo]
    [UsedImplicitly]
    public class Dead : State
    {
        private Rigidbody rb;

        public override void Init(Message initiator)
        {
            rb = actor.GetComponent<Rigidbody>();
            if (rb == null) rb = actor.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(actor.transform.forward * 10, ForceMode.Impulse);
            rb.AddForce(actor.transform.up * 10, ForceMode.Impulse);
        }

        public override void Update()
        {
            if (Timer > 5f)
            {
                Exit();
            }
        }

        public override void End()
        {
            Object.Destroy(rb);
            Transform transform = actor.transform;
            transform.position = new(6.5f, 2f, -17f);
            transform.rotation = Quaternion.identity;
        }
    }
}