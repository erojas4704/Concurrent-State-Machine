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
            Assert.Throws<CsmException>(() => { actor.EnterState<State13>(); });
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
    }
}