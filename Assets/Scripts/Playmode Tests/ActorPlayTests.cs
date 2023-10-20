using System.Collections;
using CSM;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Playmode_Tests
{
    public class ActorPlayTests
    {
        private GameObject go;
        private Actor actor;

        [SetUp]
        public void SetUp()
        {
            go = new();
            actor = go.AddComponent<Actor>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
            actor = null;
        }

        [UnityTest]
        public IEnumerator TestStateTimer()
        {
            actor.EnterState<Transitioning>();
            actor.Update();

            yield return new WaitForSeconds(1f);

            actor.Update();
            Assert.IsFalse(actor.Is<Transitioning>());
        }

        [UnityTest]
        public IEnumerator GhostStateShouldNotProcessInputAfterExpiration()
        {
            // Assert.IsTrue(false);
            Message jumpMessage = new Message("Jump", Message.Phase.Started);
            actor.EnterState<GroundedState>();
            actor.Update();
            GroundedState groundedState = actor.GetState<GroundedState>();
            groundedState.isTouchingGround = false;
            actor.Update();
            
            Assert.IsFalse(actor.Is<GroundedState>());
            actor.EnqueueMessage(jumpMessage);
            actor.Update(); //Ghost state at this point processes jump
            Assert.AreEqual(1, groundedState.jumps);
            
            yield return new WaitForSeconds(0.2f);
            actor.EnqueueMessage(jumpMessage);
            actor.Update(); //Ghost state at this point should not process jump
            Assert.AreEqual(1, groundedState.jumps);
        }


        [UsedImplicitly]
        private class Transitioning : State
        {
            public override void Update()
            {
                if (Timer > 1f)
                {
                    Exit();
                }
            }
        }

        [UsedImplicitly]
        private class GroundedState : State
        {
            public bool isTouchingGround;
            public int jumps;

            public override void Update()
            {
                if (!isTouchingGround)
                    Exit(0.15f);
            }

            public override bool Process(Message message)
            {
                if (message.name == "Jump" && message.phase == Message.Phase.Started)
                {
                    jumps++;
                }

                return false;
            }
        }
    }
}