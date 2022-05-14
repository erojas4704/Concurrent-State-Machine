using CSM;
using UnityEngine;
using System;

namespace CSM.States
{
    [Serializable]
    [StateDescriptor(priority = 98, group = -1)]
    public class InMotion : State
    {
        private Entity entity;
        private CharacterController controller;

        override public void Init(Actor actor)
        {
            entity = actor.GetComponent<Entity>();
            controller = actor.GetComponent<CharacterController>();
        }

        override public void Update(Actor actor)
        {
            controller.Move(entity.Velocity * Time.deltaTime);
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