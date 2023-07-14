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
    }
}