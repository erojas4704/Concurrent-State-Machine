using CSM;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class MessagingTest
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

        [Test]
        public void TestShouldBeginSprint()
        {
            actor.EnterState<GroundedState>();

            actor.Update();
            actor.PropagateMessage(new("Sprint", Message.Phase.Started));

            actor.Update();
            Assert.IsTrue(actor.Is<MovingState>());
            Assert.IsTrue(actor.Is<GroundedState>());
            Assert.IsTrue(actor.Is<SprintState>());
        }

        [Test]
        public void TestShouldBeginSprintingAfterAttackEnds()
        {
            actor.EnterState<GroundedState>();

            actor.Update();
            actor.PropagateMessage(new("Sprint", Message.Phase.Started));
            actor.PropagateMessage(new("Sprint", Message.Phase.Held));


            actor.Update();
            Assert.IsTrue(actor.Is<MovingState>());
            Assert.IsTrue(actor.Is<GroundedState>());
            Assert.IsTrue(actor.Is<SprintState>());
            actor.PropagateMessage(new("Attack", Message.Phase.Started));

            actor.Update();
            Assert.IsTrue(actor.Is<AttackState>());
            Assert.IsFalse(actor.Is<SprintState>());

            AttackState attackState = actor.GetState<AttackState>();
            attackState.shouldEnd = true;
            actor.Update();
            actor.Update();

            Assert.IsFalse(actor.Is<AttackState>());
            Assert.IsTrue(actor.Is<SprintState>());
        }

        [Test]
        public void TestStateBlockingMessages()
        {
            actor.EnterState<GroundedState>();
            actor.Update();

            actor.PropagateMessage(new("Attack", Message.Phase.Started));
            actor.Update();

            Assert.IsTrue(actor.Is<AttackState>());
            Message jumpMessage = new("Jump", Message.Phase.Started);
            actor.PropagateMessage(jumpMessage);

            actor.Update();
            Assert.IsFalse(actor.Is<JumpState>());
            Assert.IsFalse(actor.Is<AirborneState>());
            Assert.IsFalse(jumpMessage.processed);
            Assert.IsTrue(actor.Is<GroundedState>());
        }

        [StateDescriptor(group = 2, priority = 99)]
        [Require(typeof(GroundedState))]
        private class AttackState : State
        {
            public bool shouldEnd = false;

            public override void Update()
            {
                if (shouldEnd) Exit();
            }

            public override bool Process(Message message)
            {
                return true; //This state blocks further messages from passing down the chain.
            }
        }

        [StateDescriptor(group = 2)]
        [Require(typeof(GroundedState))]
        private class SprintState : State { }

        [With(typeof(MovingState))]
        private class GroundedState : State
        {
            public override bool Process(Message message)
            {
                if (message.phase is Message.Phase.Started or Message.Phase.Held)
                {
                    if (message.name == "Sprint")
                    {
                        actor.EnterState<SprintState>();
                        message.processed = true;
                    }

                    if (message.name == "Attack")
                    {
                        actor.EnterState<AttackState>();
                        message.processed = true;
                    }

                    if (message.name == "Jump")
                    {
                        actor.EnterState<JumpState>();
                        message.processed = true;
                    }
                }

                return false;
            }
        }

        [With(typeof(AirborneState))]
        private class JumpState : State { }

        [With(typeof(MovingState))]
        [StateDescriptor(group = 1)]
        private class AirborneState : State { }

        private class MovingState : State { }
    }
}