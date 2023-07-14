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

        [Test]
        public void TestGhostStateProcessesMessage()
        {
            actor.EnterState<GroundedState>();

            actor.Update();
            GroundedState groundedState = actor.GetState<GroundedState>();
            groundedState.isTouchingGround = false;

            actor.Update();
            actor.Update();
            Assert.IsFalse(actor.Is<GroundedState>());
            Assert.IsTrue(actor.Is<AirborneState>());

            Message message = new("Jump", Message.Phase.Started);
            actor.PropagateMessage(message);

            actor.Update();
            Assert.IsTrue(actor.Is<JumpState>());
            Assert.IsTrue(message.processed);
        }

        [Test]
        public void TestGhostStatesShouldNotProcessMessagesMoreThanOnce()
        {
            Message message1 = new("Jump", Message.Phase.Started);
            Message message2 = new Message("Jump", Message.Phase.Started);
            actor.EnterState<GroundedState>();
                
            actor.Update();
            GroundedState groundedState = actor.GetState<GroundedState>();
            groundedState.isTouchingGround = false;
            
            actor.Update();
            actor.Update();
            Assert.IsFalse(actor.Is<GroundedState>());
            Assert.IsTrue(actor.Is<AirborneState>());
            actor.PropagateMessage(message1);
            
            actor.Update();
            Assert.IsTrue(actor.Is<JumpState>());
            actor.PropagateMessage(message2);
            
            Assert.IsTrue(message1.processed);
            Assert.IsFalse(message2.processed);
        }

        [Test]
        public void TestMessageShouldNotEndGhostState()
        {
            actor.EnterState<GroundedState>();

            actor.Update();
            GroundedState groundedState = actor.GetState<GroundedState>();
            groundedState.isTouchingGround = false;

            actor.Update();
            actor.Update();
            Assert.IsFalse(actor.Is<GroundedState>());
            Assert.IsTrue(actor.Is<AirborneState>());

            Message message = new("Jump", Message.Phase.Started);
            actor.PropagateMessage(new("Axis", Message.Phase.Started));
            actor.PropagateMessage(new("Axis", Message.Phase.Held));
            actor.PropagateMessage(message);

            actor.Update();
            Assert.IsTrue(actor.Is<JumpState>());
            Assert.IsTrue(message.processed);
        }

        [Test]
        public void TestBlockingStatesShouldNotKeepInputs() 
        {
            //Z-90
            actor.EnterState<GroundedState>();

            actor.Update();
            actor.PropagateMessage(new("Sprint", Message.Phase.Started));
            actor.PropagateMessage(new("Sprint", Message.Phase.Held));
            
            actor.Update();
            Assert.IsTrue(actor.Is<SprintState>());
            actor.PropagateMessage(new("Attack", Message.Phase.Started));

            actor.Update();
            Assert.IsTrue(actor.Is<AttackState>());
            Assert.IsFalse(actor.Is<SprintState>());
            
            actor.Update();
            actor.PropagateMessage(new("Sprint", Message.Phase.Ended));

            actor.Update();
            Assert.IsFalse(actor.Is<SprintState>());
            AttackState attackState = actor.GetState<AttackState>();
            attackState.shouldEnd = true;
            
            actor.Update(); //Attack State Ends 
            actor.Update(); //Actor removes attack state
            actor.Update(); //Actor processes held input
            Assert.IsFalse(actor.Is<SprintState>());
        }

        [StateDescriptor(group = 2, priority = 99)]
        [Require(typeof(GroundedState))]
        private class AttackState : State
        {
            public bool shouldEnd;

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
        [StateDescriptor(group = 1)]
        private class GroundedState : State
        {
            public bool isTouchingGround = true;

            public override void Update()
            {
                if (!isTouchingGround)
                {
                    actor.EnterState<AirborneState>();
                    actor.Persist(this, 2f);
                }
            }

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

                    if (message.name == "Axis")
                    {
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