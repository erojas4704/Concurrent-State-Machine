using System;
using CSM;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

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

            //Sprint is held, actor enters "SprintState"
            actor.Update();
            Assert.IsTrue(actor.Is<MovingState>());
            Assert.IsTrue(actor.Is<GroundedState>());
            Assert.IsTrue(actor.Is<SprintState>());
            actor.PropagateMessage(new("Attack", Message.Phase.Started));

            //Actor enters "AttackState", which negates the "SprintState"
            actor.Update();
            Assert.IsTrue(actor.Is<AttackState>());
            Assert.IsFalse(actor.Is<SprintState>());

            AttackState attackState = actor.GetState<AttackState>();
            attackState.shouldEnd = true;
            
            //Attack state ends, actor should re-enter "SprintState"
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
            Message axisMessage0 = new("Axis", Message.Phase.Started);
            Message axisMessage1 = new("Axis", Message.Phase.Held);
            axisMessage0.SetValue(Vector2.up);
            axisMessage1.SetValue(Vector2.up);
            actor.PropagateMessage(axisMessage0);
            actor.PropagateMessage(axisMessage1);
            actor.PropagateMessage(message);

            actor.Update();
            Assert.IsTrue(actor.Is<JumpState>());
            Assert.IsTrue(message.processed);
        }

        [Test]
        public void TestBlockingStatesShouldNotKeepInputs()
        {
            //Addresses bug in Z-90
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

        [Test]
        public void TestHeldAxisShouldPropagateToMovementState()
        {
            Message axisMessage = new("Axis", Message.Phase.Started);
            Vector2 movementAxis = new(-1f, 0f);
            axisMessage.SetValue(movementAxis);
            actor.EnterState<GroundedState>();

            actor.Update();
            actor.PropagateMessage(axisMessage);
            MovingState movingState = actor.GetState<MovingState>();
            Assert.AreEqual(movementAxis, movingState.axis);
            Assert.IsTrue(actor.Is<MovingState>());
            Assert.IsTrue(actor.Is<GroundedState>());
        }

        [Test]
        public void TestHeldAxisShouldPropagateToIncomingState()
        {
            Message axisMessage = new("Axis", Message.Phase.Held);
            Vector2 movementAxis = new(-1f, 0f);
            axisMessage.SetValue(movementAxis);
            actor.EnterState<ClimbState>();

            actor.Update();
            actor.PropagateMessage(axisMessage);

            actor.Update();
            actor.EnterState<GroundedState>();

            actor.Update();
            MovingState movingState = actor.GetState<MovingState>();
            Assert.AreEqual(movementAxis, movingState.axis);
        }

        [Test]
        public void TestHeldAxisShouldPassAfterBlockingStateEnds()
        {
            Message axisMessage = new("Axis", Message.Phase.Held);
            Vector2 movementAxis = new(-1f, 0f);
            axisMessage.SetValue(movementAxis);
            actor.EnterState<GroundedState>();

            actor.Update();
            actor.EnterState<BlockingState>();

            actor.Update();
            actor.PropagateMessage(axisMessage);

            actor.Update();
            Assert.IsFalse(axisMessage.processed);
            actor.ExitState<BlockingState>();

            actor.Update();
            Assert.IsFalse(actor.Is<BlockingState>());
            Assert.IsTrue(actor.Is<GroundedState>());
            Assert.IsTrue(actor.Is<MovingState>());

            MovingState movingState = actor.GetState<MovingState>();
            Assert.AreEqual(movementAxis, movingState.axis);
        }

        [Test]
        public void TestGhostStateShouldNotExistWithLiveState()
        {
            actor.EnterState<GroundedState>();

            actor.Update();
            GroundedState groundedState = actor.GetState<GroundedState>();
            groundedState.isTouchingGround = false;
            actor.Update();

            actor.EnterState<GroundedState>();
            actor.PropagateMessage(new("Jump", Message.Phase.Started));
            actor.Update();
            actor.Update();
            Assert.AreEqual(1, groundedState.jumps);
        }

        [Test]
        public void TestStateStartedWithInitiator()
        {
            actor.EnterState<StateWithInitiator>("Test");

            actor.Update();
            StateWithInitiator stateWithInitiator = actor.GetState<StateWithInitiator>();
            Assert.AreEqual("Test", stateWithInitiator.initiatorData);
        }

        [Test]
        public void TestAxisValuePassedThroughMessage()
        {
            actor.EnterState<AxisState>();

            actor.Update();
            Message axisMessage = new Message("Axis", Message.Phase.Started);
            axisMessage.SetValue(Vector2.right);
            actor.PropagateMessage(axisMessage);

            actor.Update();

            AxisState axisState = actor.GetState<AxisState>();
            Assert.AreEqual(Vector2.right, axisState.axis);
            Assert.IsTrue(axisMessage.processed);
            Assert.IsTrue(actor.Is<AxisState>());
        }

        [Test]
        public void TestAxisValuePassedThroughMessageModifyAndPropagate()
        {
            actor.EnterState<AxisState>();
            actor.EnterState<AxisFlipState>();

            actor.Update();
            Message axisMessage = new Message("Axis", Message.Phase.Started);
            axisMessage.SetValue(Vector2.right);
            actor.PropagateMessage(axisMessage);

            actor.Update();

            AxisState axisState = actor.GetState<AxisState>();
            Assert.AreEqual(Vector2.left, axisState.axis);
            Assert.IsTrue(axisMessage.processed);
            Assert.IsTrue(actor.Is<AxisState>());
            Assert.IsTrue(actor.Is<AxisFlipState>());
        }


        #region messaging states

        [StateDescriptor(priority =  2)]
        private class AxisFlipState : State
        {
            public override bool Process(Message message)
            {
                if (message.name == "Axis" && message.phase == Message.Phase.Started)
                {
                    Vector2 axis = message.GetValue<Vector2>();
                    axis = -axis;
                    message.SetValue(axis);
                    message.processed = true;
                }

                return false;
            }
        }

        private class AxisState : State
        {
            public Vector2 axis;

            public override bool Process(Message message)
            {
                if (message.name == "Axis" && message.phase == Message.Phase.Started)
                {
                    axis = message.GetValue<Vector2>();
                    message.processed = true;
                }

                return false;
            }
        }

        private class StateWithInitiator : State
        {
            public string initiatorData;

            public override void Init(Message initiator)
            {
                initiatorData = initiator.GetTrigger<String>();
            }
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
            public int jumps;

            public override void Update()
            {
                if (!isTouchingGround)
                {
                    actor.EnterState<AirborneState>();
                    Exit(2f, "Jump");
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
                        jumps++;
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

        private class MovingState : State
        {
            public Vector2 axis;

            public override bool Process(Message message)
            {
                if (message.name == "Axis" && message.phase is Message.Phase.Started or Message.Phase.Held)
                {
                    axis = message.GetValue<Vector2>();
                }

                return false;
            }
        }

        [StateDescriptor(group = 1, priority = 6)]
        private class ClimbState : State
        {
            public override bool Process(Message message)
            {
                return true;
            }
        }

        [StateDescriptor(priority = 99)]
        private class BlockingState : State
        {
            public override bool Process(Message message)
            {
                return true;
            }
        }

        #endregion
    }
}