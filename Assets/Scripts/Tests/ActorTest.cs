using CSM;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tests
{
    public class ActorTest
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

        #region dependency tests

        [Test]
        public void TestActorSimpleState()
        {
            actor.EnterState<State0>();
            actor.Update();

            Assert.IsTrue(actor.Is<State0>());
        }

        [Test]
        public void TestActorWithPartnerStates()
        {
            actor.EnterState<State1>();
            actor.Update();

            Assert.IsTrue(actor.Is<State0>());
            Assert.IsTrue(actor.Is<State2>());
        }

        [Test]
        public void TestActorWithSoloStates()
        {
            actor.EnterState<State0>();
            actor.EnterState<State1>();
            actor.Update();

            Assert.IsTrue(actor.Is<State0>());
            Assert.IsTrue(actor.Is<State1>());

            actor.EnterState<State8>();
            actor.Update();

            Assert.IsTrue(actor.Is<State8>());
            Assert.IsFalse(actor.Is<State0>());
            Assert.IsFalse(actor.Is<State1>());
            Assert.AreEqual(actor.GetStates().Count, 1);
        }

        [Test]
        public void TestComplexStateDependencyChains()
        {
            actor.EnterState<State3>();
            //Should Fail because 3 triggers 4 and 5. 4 Requires 6 and 5 requires 7.

            actor.Update();
            Assert.IsFalse(actor.Is<State3>());
            Assert.IsFalse(actor.Is<State4>());
            Assert.IsFalse(actor.Is<State5>());
            Assert.IsFalse(actor.Is<State6>());
            Assert.IsFalse(actor.Is<State7>());

            actor.EnterState<State6>();
            actor.EnterState<State7>();
            actor.Update(); //TODO <- Refactor Dependency Checks to check the incoming stack.

            actor.EnterState<State3>();

            actor.Update();
            Assert.IsTrue(actor.Is<State3>());
            Assert.IsTrue(actor.Is<State4>());
            Assert.IsTrue(actor.Is<State5>());
            Assert.IsTrue(actor.Is<State6>());
            Assert.IsTrue(actor.Is<State7>());

            Assert.AreEqual(actor.GetStates().Count, 5);
        }

        [Test]
        public void TestRequiredStates()
        {
            //Should Fail
            actor.EnterState<State11>();

            actor.Update();
            Assert.IsFalse(actor.Is<State11>());

            //Try again
            actor.EnterState<State1>();
            actor.EnterState<State2>();
            actor.EnterState<State11>();


            //TODO <- State requirements should check the incoming state queue too.
            //This might fail until we address this

            actor.Update();
            Assert.IsTrue(actor.Is<State1>());
            Assert.IsTrue(actor.Is<State2>());
            Assert.IsTrue(actor.Is<State11>());
        }

        [Test]
        public void TestSimpleStateNegation()
        {
            actor.EnterState<State1>();
            actor.EnterState<State2>();

            actor.Update();
            Assert.IsTrue(actor.Is<State1>());
            Assert.IsTrue(actor.Is<State2>());
            actor.EnterState<State9>();

            actor.Update();
            Assert.IsTrue(actor.Is<State9>());
            Assert.IsFalse(actor.Is<State1>());
            Assert.IsFalse(actor.Is<State2>());
        }

        [Test]
        public void TestNegationWithNestedPartnering()
        {
            actor.EnterState<State1>();
            actor.EnterState<State2>();

            actor.Update();
            Assert.IsTrue(actor.Is<State1>());
            Assert.IsTrue(actor.Is<State2>());

            actor.EnterState<State10>();

            actor.Update();
            Assert.IsTrue(actor.Is<State9>());
            Assert.IsFalse(actor.Is<State1>());
            Assert.IsFalse(actor.Is<State2>());
        }

        [Test]
        public void TestComplexNestedNegation()
        {
            actor.EnterState<State0>();
            actor.EnterState<State1>();
            actor.EnterState<State2>();
            actor.EnterState<State7>();

            actor.Update();
            Assert.IsTrue(actor.Is<State0>());
            Assert.IsTrue(actor.Is<State1>());
            Assert.IsTrue(actor.Is<State2>());
            Assert.IsTrue(actor.Is<State7>());

            actor.EnterState<State12>();
            actor.Update();
            Assert.IsTrue(actor.Is<State9>());
            Assert.IsTrue(actor.Is<State10>());
            Assert.IsTrue(actor.Is<State12>());

            Assert.IsFalse(actor.Is<State0>());
            Assert.IsFalse(actor.Is<State1>());
            Assert.IsFalse(actor.Is<State2>());
            Assert.IsFalse(actor.Is<State7>());
            Assert.AreEqual(actor.GetStates().Count, 3);
        }

        [Test]
        public void TestRapidStateChanges()
        {
            actor.EnterState<State0>();
            actor.ExitState<State0>();
            actor.EnterState<State1>();
            actor.ExitState<State1>();
            actor.Update();

            Assert.IsFalse(actor.Is<State0>());
            Assert.IsFalse(actor.Is<State1>());
            Assert.AreEqual(actor.GetStates().Count, 0);
        }

        [Test]
        public void TestRapidStateChangesWithComplexNegation()
        {
            actor.EnterState<State35>(); //Includes 36. 36 requires 35.
            actor.EnterState<State32>(); //Includes 33. 33 Includes 34.  34 Negates 35.

            actor.Update();
            //35 and 36 should be gone.
            Assert.IsFalse(actor.Is<State35>());
            Assert.IsFalse(actor.Is<State36>());
            Assert.IsTrue(actor.Is<State32>());
            Assert.IsTrue(actor.Is<State33>());
            Assert.IsTrue(actor.Is<State34>());
            Assert.AreEqual(actor.GetStates().Count, 3);
        }

        //TODO: Test grouping and priority.
        [Test]
        public void TestGroupedStates()
        {
            actor.EnterState<State22>();
            actor.Update();

            Assert.IsTrue(actor.Is<State22>());
            actor.EnterState<State23>();

            actor.Update();
            Assert.IsFalse(actor.Is<State22>());
            Assert.IsTrue(actor.Is<State23>());
            Assert.AreEqual(actor.GetStates().Count, 1);
        }

        [Test]
        public void TestUnsolvableDependencyChain()
        {
            //Should throw CSM Exception.
            actor.EnterState<State13>();
            Assert.Throws<CsmException>(() => { actor.Update(); });
            Assert.AreEqual(actor.GetStates().Count, 0);
        }

        [Test]
        public void TestInvalidStateRelationships()
        {
            //This should fail and throw an CSMException
            Assert.Throws<CsmException>(() => actor.EnterState<State16>());
            Assert.Throws<CsmException>(() => actor.EnterState<State17>());
            Assert.Throws<CsmException>(() => actor.EnterState<State18>());
        }

        [Test]
        public void TestImpossibleStateGroupShouldThrowException()
        {
            Assert.Throws<CsmException>(() => actor.EnterState<State30>());
        }

        [Test]
        public void TestStateWithCircularDependencies()
        {
            //This would cause an infinite loop if poorly implemented. Make sure it does not. 
            actor.EnterState<State19>();

            actor.Update();
            Assert.IsTrue(actor.Is<State19>());
            Assert.IsTrue(actor.Is<State20>());
        }

        [Test]
        public void TestExitState()
        {
            actor.EnterState<State0>();
            actor.Update();

            Assert.IsTrue(actor.Is<State0>());
            actor.ExitState<State0>();

            actor.Update();
            Assert.IsFalse(actor.Is<State0>());
            Assert.AreEqual(actor.GetStates().Count, 0);
        }

        [Test]
        public void TestExitRequiredState()
        {
            //Tests exiting a state that is required by others. 
            //The states requiring that state should also be exited.
            actor.EnterState<State1>();
            actor.EnterState<State11>();

            actor.Update();
            Assert.IsTrue(actor.Is<State0>()); // Included by State1
            Assert.IsTrue(actor.Is<State1>());
            Assert.IsTrue(actor.Is<State2>()); // State 1 Includes 0 and 3
            Assert.IsTrue(actor.Is<State11>());

            actor.ExitState<State1>();
            actor.Update();

            Assert.IsFalse(actor.Is<State11>()); //Actor should have exited State11 after exiting State1.
        }

        [Test]
        public void TestExitRequiredStateWithComplexDependencies()
        {
            actor.EnterState<State24>();
            actor.EnterState<State25>();
            actor.EnterState<State26>();
            actor.EnterState<State27>();

            actor.Update();
            Assert.IsTrue(actor.Is<State24>());
            Assert.IsTrue(actor.Is<State25>());
            Assert.IsTrue(actor.Is<State26>());
            Assert.IsTrue(actor.Is<State27>());

            actor.ExitState<State24>();

            actor.Update();
            Assert.IsFalse(actor.Is<State24>());
            Assert.IsFalse(actor.Is<State25>());
            Assert.IsFalse(actor.Is<State26>());
            Assert.IsFalse(actor.Is<State27>());
            Assert.AreEqual(actor.GetStates().Count, 0);
        }

        [Test]
        public void TestMultipleSoloStates()
        {
            actor.EnterState<State8>();
            actor.EnterState<State21>();

            actor.Update();
            Assert.IsFalse(actor.Is<State8>());
            Assert.IsTrue(actor.Is<State21>());
            Assert.AreEqual(actor.GetStates().Count, 1);
        }

        [Test]
        public void TestGroupedStatesWithRequirements()
        {
            actor.EnterState<State22>();
            actor.EnterState<State28>();
            actor.EnterState<State29>();

            actor.Update();
            actor.EnterState<State23>();

            actor.Update();
            Assert.IsFalse(actor.Is<State22>());
            Assert.IsFalse(actor.Is<State28>());
            Assert.IsFalse(actor.Is<State29>());
            Assert.IsTrue(actor.Is<State23>());
        }

        [Test]
        public void TestNegatedStatesShouldBeInaccessible()
        {
            actor.EnterState<State15>();
            actor.Update();

            actor.EnterState<State14>();
            actor.Update();

            Assert.IsTrue(actor.Is<State15>());
            Assert.IsFalse(actor.Is<State14>());
        }

        #endregion
        
        [Test]
        public void TestEnterStateCheckRequirementsSameCycle()
        {
            //Requirements should be met from incoming states
            actor.EnterState<Sprint>(); 
            actor.EnterState<Grounded>();
            
            actor.Update();
            Assert.IsTrue(actor.Is<Sprint>());
            Assert.IsTrue(actor.Is<Grounded>());
        }
        
        [Test]
        public void TestMultipleEnterStateCalls()
        {
            actor.EnterState<Grounded>();
            
            actor.Update();
            actor.EnterState<Sprint>(); 
            actor.EnterState<Sprint>(); 
            
            actor.Update();
            Assert.IsTrue(actor.Is<Sprint>());
            Assert.IsTrue(actor.Is<Grounded>());
        }


        #region messaging tests

        [Test]
        public void TestGhostStateShouldNotTriggerWhenStateIsReplaced()
        {
            Message message1 = new("End", Message.Phase.Started);
            Message message2 = new("End", Message.Phase.Started);
            actor.EnterState<MessagingState0>();

            actor.Update();
            actor.PropagateMessage(message1);

            actor.Update();
            Assert.IsTrue(message1.processed);
            Assert.IsTrue(actor.Is<MessagingState1>());
            Assert.IsFalse(actor.Is<MessagingState0>());

            actor.PropagateMessage(message2);
            actor.Update();
            Assert.IsFalse(message2.processed);
        }

        [Test]
        public void TestGhostStateShouldProcessMessageAfterExit()
        {
            Message message1 = new("End", Message.Phase.Started);
            actor.EnterState<MessagingState0>();

            actor.Update();
            MessagingState0 messagingState0 = actor.GetStates()[typeof(MessagingState0)] as MessagingState0;
            messagingState0.shouldEndAndEnterMessagingState1 = true;
            actor.Update(); //This update makes sure the State processes the above prompt.

            actor.Update();
            Assert.IsFalse(actor.Is<MessagingState0>());
            Assert.IsTrue(actor.Is<MessagingState1>());

            actor.PropagateMessage(message1);
            actor.Update();

            Assert.IsTrue(message1.processed);
            Assert.LessOrEqual(messagingState0.updates, 2);
        }

        [Test]
        public void TestGhostStateShouldNotProcessMessageAfterCancelledByGroup()
        {
            Message message1 = new("Jump", Message.Phase.Started);
            Message message2 = new("Jump", Message.Phase.Started);
            actor.EnterState<Grounded>();

            actor.Update();
            actor.PropagateMessage(message1); //"Jump" is sent to grounded.

            actor.Update(); //This update needs to be the one to resolve Jump, Airborne, but not Grounded.
            
            //Grounded forces actor to enter Jump, which forces Airborne.
            //This should have cancelled Grounded.
            
            Assert.IsFalse(actor.Is<Grounded>());
            Assert.IsTrue(actor.Is<Airborne>());
            Assert.IsTrue(actor.Is<Jump>());

            actor.Update();
            actor.PropagateMessage(message2);
            
            actor.Update();
            Assert.IsTrue(message1.processed);
            Assert.IsFalse(message2.processed);
            Assert.AreEqual(actor.GetStates().Count, 2);
        }
        
        [Test]
        public void TestGhostStateShouldProcessJump()
        {            
            Message message1 = new("Jump", Message.Phase.Started);
            actor.EnterState<Grounded>();
            
            actor.Update();
            Grounded groundedState = actor.GetState<Grounded>();
            groundedState.isTouchingGround = false;
            
            actor.Update();
            //The State will not be removed until the 2nd update later because it removes itself. 
            //Should we raise a concern over this?
            
            actor.Update();
            Assert.IsFalse(actor.Is<Grounded>());
            actor.PropagateMessage(message1);
            
            actor.Update();
            Assert.IsFalse(actor.Is<Grounded>());
            Assert.IsTrue(actor.Is<Jump>());
            Assert.IsTrue(actor.Is<Airborne>());
            Assert.IsTrue(message1.processed);
        }

        [Test]
        public void TestHeldInputShouldBeProcessedByNewState()
        {            
            Message message1 = new("Sprint", Message.Phase.Started);
            Message message2 = new("Sprint", Message.Phase.Held);
            Message message3 = new("Sprint", Message.Phase.Ended);
            Message message4 = new("Jump", Message.Phase.Started);
            actor.EnterState<Grounded>();
            
            actor.Update();
            actor.PropagateMessage(message1); //Press Sprint
            
            actor.Update();
            actor.PropagateMessage(message2); //Holding Sprint

            Assert.IsTrue(actor.Is<Sprint>());
            actor.PropagateMessage(message4); //Press Jump
            actor.Update();
            
            Assert.IsTrue(actor.Is<Airborne>());
            Assert.IsFalse(actor.Is<Sprint>());
            
            actor.Update();
            actor.EnterState<Grounded>(); 
            
            actor.Update(); //Landed
            Assert.IsTrue(actor.Is<Sprint>());
            Assert.IsTrue(actor.Is<Grounded>());

            actor.PropagateMessage(message3);
            actor.Update();
            
            Assert.IsFalse(actor.Is<Sprint>());
            Assert.IsTrue(actor.Is<Grounded>());
        }
        
        #endregion

        #region dependency states

        /** State0 Has no Requirements */
        private class State0 : State
        {
        }

        /** State1 Includes State0 and State2. */
        [With(typeof(State0), typeof(State2))]
        private class State1 : State
        {
        }

        /** State2 is a simple State */
        private class State2 : State
        {
        }

        /** State3 Includes State4 and State5. State4 requires State6 and State5 requires State7. */
        [With(typeof(State4), typeof(State5))]
        private class State3 : State
        {
        }

        /** State4 requires State6 */
        [Require(typeof(State6))]
        private class State4 : State
        {
        }

        /** State5 requires State7 */
        [Require(typeof(State7))]
        private class State5 : State
        {
        }

        /** Simple State */
        private class State6 : State
        {
        }

        /** Simple State */
        private class State7 : State
        {
        }

        /** State8 Includes that no other states be present. State8 will eliminate all other states. */
        [Solo]
        private class State8 : State
        {
        }

        /** State9 Negates States 1 and 2 */
        [Negate(typeof(State1), typeof(State2))]
        private class State9 : State
        {
        }

        /** State10 Includes State9, which negates States 1 and 2*/
        [With(typeof(State9))]
        private class State10 : State
        {
        }

        /** State11 Requires States 1, and 2 */
        [Require(typeof(State1), typeof(State2))]
        private class State11 : State
        {
        }

        /** State12 Includes State 10, which includes State9 which negates states 1 and 2. State12 Negates States 0 and 7 */
        [With(typeof(State10)), Negate(typeof(State0), typeof(State7))]
        private class State12 : State
        {
        }

        /** State13 Includes State14 and State15. State15 Negates State 14. This state is unsolvable */
        [With(typeof(State14), typeof(State15))]
        private class State13 : State
        {
        }

        private class State14 : State
        {
        }

        [Negate(typeof(State14))]
        private class State15 : State
        {
        }

        /** Requires Itself */
        [Require(typeof(State16))]
        private class State16 : State
        {
        }

        /** Negates itself */
        [Negate(typeof(State17))]
        private class State17 : State
        {
        }

        /** Includes itself */
        [With(typeof(State18))]
        private class State18 : State
        {
        }

        /** Includes 20, which includes 19 */
        [With(typeof(State20))]
        private class State19 : State
        {
        }

        /** Includes 19, which includes 20. */
        [With(typeof(State19))]
        private class State20 : State
        {
        }

        [Solo]
        private class State21 : State
        {
        }

        [StateDescriptor(group = 1)]
        private class State22 : State
        {
        }

        [StateDescriptor(group = 1)]
        private class State23 : State
        {
        }

        private class State24 : State
        {
        }

        [Require(typeof(State24))]
        private class State25 : State
        {
        }

        [Require(typeof(State24))]
        private class State26 : State
        {
        }

        [Require(typeof(State25))]
        private class State27 : State
        {
        }

        [Require(typeof(State22))]
        private class State28 : State
        {
        }

        [Require(typeof(State22))]
        private class State29 : State
        {
        }

        [With(typeof(State31)), StateDescriptor(group = 1)]
        private class State30 : State
        {
        }

        [StateDescriptor(group = 1)]
        private class State31 : State
        {
        }

        [With(typeof(State33))]
        private class State32 : State
        {
        }

        [With(typeof(State34))]
        private class State33 : State
        {
        }

        [Negate(typeof(State35))]
        private class State34 : State
        {
        }

        [With(typeof(State36))]
        private class State35 : State
        {
        }

        [Require(typeof(State35))]
        private class State36 : State
        {
        }

        #endregion

        #region messaging states

        [StateDescriptor(group = 1)]
        private class MessagingState0 : State
        {
            public bool shouldEndAndEnterMessagingState1;
            public int updates;

            public override void Update()
            {
                updates++;
                if (shouldEndAndEnterMessagingState1)
                {
                    actor.EnterState<MessagingState1>();
                    Exit(2f);
                }
            }

            public override bool Process(Message message)
            {
                if (message.phase == Message.Phase.Started)
                {
                    if (message.name == "End")
                    {
                        actor.EnterState<MessagingState1>();
                        message.processed = true;
                    }
                }

                return false;
            }
        }

        [StateDescriptor(group = 1)]
        private class MessagingState1 : State
        {
        }

        [StateDescriptor(group = 1)]
        private class Grounded : State
        {
            public bool isTouchingGround = true;

            public override void Update()
            {
                if (!isTouchingGround)
                {
                    Exit(2f);
                    actor.EnterState<Airborne>();
                }
            }

            public override bool Process(Message message)
            {
                if (message.phase == Message.Phase.Started)
                {
                    switch (message.name)
                    {
                        case "Jump":
                            actor.EnterState<Jump>();
                            message.processed = true;
                            break;
                    }
                }

                if (message.phase != Message.Phase.Ended)
                {
                    switch (message.name)
                    {
                        case "Sprint":
                            actor.EnterState<Sprint>();
                            message.processed = true;
                            break;
                    }
                }

                return false;
            }
        }

        [StateDescriptor(group = 1)]
        private class Airborne : State
        {
            public override void Update()
            {
                if (actor.GetStates().TryGetValue(typeof(Grounded), out State state))
                {
                    Grounded groundedState = state as Grounded;
                    groundedState.isTouchingGround = false;
                }
            }
        }

        [Require(typeof(Grounded))]
        [StateDescriptor(group=3)]
        private class Sprint : State
        {
            public override bool Process(Message message)
            {
                if (message.name == "Sprint" && message.phase == Message.Phase.Ended)
                {
                    message.processed = true;
                    Exit();
                }

                return false;
            }
        }

        [With(typeof(Airborne))]
        private class Jump : State
        {
        }

        #endregion
    }
}