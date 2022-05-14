using CSM.Entities;
using UnityEngine;
using System;

namespace CSM.States
{
    [StateDescriptor(priority = 98, group = -1)]
    public class InMotion : State
    {
        private Entity entity;
        private CharacterController controller;

        override public void Init(Actor actor)
        {
            entity = (Entity) actor;
            controller = actor.GetComponent<CharacterController>();//
        }

        override public void Update(Actor actor)
        {
            controller.Move(entity.velocity * Time.deltaTime);
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