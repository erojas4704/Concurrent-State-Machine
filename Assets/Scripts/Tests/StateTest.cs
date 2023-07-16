using System.Linq;
using System.Reflection;
using CSM;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tests
{
    public class StateTest
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
        public void TestSimpleStatePriority()
        {
            actor.EnterState<State5>();
            actor.EnterState<State4>();
            actor.EnterState<State3>();
            actor.EnterState<State2>();
            actor.EnterState<State1>();
            actor.EnterState<State0>();

            actor.Update();
            State[] states = actor.GetStates().Values.ToArray();
            Assert.IsInstanceOf<State5>(states[0]);
            Assert.IsInstanceOf<State4>(states[1]);
            Assert.IsInstanceOf<State3>(states[2]);
            Assert.IsInstanceOf<State2>(states[3]);
            Assert.IsInstanceOf<State1>(states[4]);
            Assert.IsInstanceOf<State0>(states[5]);
        }

        [Test]
        public void TestSimpleStatePriorityRandomOrder()
        {
            actor.EnterState<State0>();
            actor.EnterState<State4>();
            actor.EnterState<State5>();
            actor.EnterState<State3>();
            actor.EnterState<State2>();
            actor.EnterState<State1>();

            actor.Update();
            State[] states = actor.GetStates().Values.ToArray();
            Assert.IsInstanceOf<State5>(states[0]);
            Assert.IsInstanceOf<State4>(states[1]);
            Assert.IsInstanceOf<State3>(states[2]);
            Assert.IsInstanceOf<State2>(states[3]);
            Assert.IsInstanceOf<State1>(states[4]);
            Assert.IsInstanceOf<State0>(states[5]);
        }

        [Test]
        public void TestStatePriorityWithRelationships()
        {
            actor.EnterState<Grounded>();

            actor.Update();
            State[] states = actor.GetStates().Values.ToArray();
            Assert.IsInstanceOf<Grounded>(states[0]);
            Assert.IsInstanceOf<Movable>(states[1]);

            actor.EnterState<Jump>();

            actor.Update();
            states = actor.GetStates().Values.ToArray();
            Assert.IsInstanceOf<DoubleJump>(states[0]);
            Assert.IsInstanceOf<Jump>(states[1]);
            Assert.IsInstanceOf<Airborne>(states[2]);
            Assert.IsInstanceOf<Movable>(states[3]);

            Assert.IsFalse(actor.Is<Grounded>());
            Assert.IsTrue(actor.Is<Movable>());
            Assert.IsTrue(actor.Is<Jump>());
            Assert.IsTrue(actor.Is<DoubleJump>());
            Assert.IsTrue(actor.Is<Airborne>());
            Assert.AreEqual(actor.GetStates().Count, 4);
        }

        [Test]
        public void TestActorDefaultState()
        {
            SetDefaultStateAndInitialize("Tests.DefaultState");
            actor.Update();
            Assert.IsTrue(actor.Is<DefaultState>());
        }

        [Test]
        public void TestActorShouldEnterDefaultStateIfStateless()
        {
            SetDefaultStateAndInitialize("Tests.DefaultState");
            actor.EnterState<Grounded>(); //<- Grounded knocks out DefaultState

            actor.Update();
            actor.ExitState<Grounded>();
            actor.ExitState<Movable>();

            actor.Update(); // Clears 2 previous states
            actor.Update(); // Enters Default State
            Assert.AreEqual(1, actor.GetStates().Count);
            Assert.IsFalse(actor.Is<Grounded>());
            Assert.IsTrue(actor.Is<DefaultState>());
        }

        [Test]
        public void TestActorShouldEnterDefaultStateWithHeldMessages()
        {
            SetDefaultStateAndInitialize("Tests.DefaultState");
            actor.EnterState<Grounded>(); //<- Grounded knocks out DefaultState

            actor.Update();
            actor.PropagateMessage(new("Jump", Message.Phase.Started));
            actor.PropagateMessage(new("Jump", Message.Phase.Held));
            
            actor.ExitState<Grounded>();
            actor.ExitState<Movable>();

            actor.Update();
            actor.Update();
            Assert.AreEqual(1, actor.GetStates().Count);
            Assert.IsFalse(actor.Is<Grounded>());
            Assert.IsTrue(actor.Is<DefaultState>());
        }

        [Test]
        public void TestActorShouldNotEnterInvalidStates()
        {
            Assert.Throws<CsmException>(() => { actor.EnterState(typeof(void)); });
        }

        [Test]
        public void TestActorShouldEnterPartnerStateWithBackRequirement()
        {
            actor.EnterState<MasterState>();
            
            actor.Update();
            Assert.IsTrue(actor.Is<MasterState>());
            Assert.IsTrue(actor.Is<AssistState>());
        }
        
        
        private void SetDefaultStateAndInitialize(string defaultStateReference)
        {
            FieldInfo defaultStateField = typeof(Actor).GetField("defaultState",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            MethodInfo awakeMethod = typeof(Actor).GetMethod("Awake",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            Assert.NotNull(defaultStateField);
            Assert.NotNull(awakeMethod);

            defaultStateField.SetValue(actor, defaultStateReference);
            awakeMethod.Invoke(actor, null);
        }

        #region test states
        [Require(typeof(Airborne))]
        private class AssistState : State {}
        
        [With(typeof(AssistState))]
        private class MasterState : State {}

        [StateDescriptor(priority = 0)]
        private class State0 : State { }

        [StateDescriptor(priority = 1)]
        private class State1 : State { }

        [StateDescriptor(priority = 2)]
        private class State2 : State { }

        [StateDescriptor(priority = 3)]
        private class State3 : State { }

        [StateDescriptor(priority = 4)]
        private class State4 : State { }

        [StateDescriptor(priority = 5)]
        private class State5 : State { }

        [StateDescriptor(priority = 0)]
        private class Movable : State { }

        [With(typeof(Movable))]
        [StateDescriptor(priority = 5, group = 1)]
        private class Grounded : State { }

        [With(typeof(Movable))]
        [StateDescriptor(priority = 5, group = 1)]
        private class Airborne : State { }

        [With(typeof(Airborne), typeof(DoubleJump))]
        [StateDescriptor(priority = 6)]
        private class Jump : State { }

        [StateDescriptor(priority = 7)]
        private class DoubleJump : State { }

        #endregion
    }

    // ReSharper disable once MemberCanBePrivate.Global
    [StateDescriptor(group = 1)]
    public class DefaultState : State { }
}