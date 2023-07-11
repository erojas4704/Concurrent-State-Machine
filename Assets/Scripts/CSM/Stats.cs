using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CSM
{
    public class Stats : MonoBehaviour
    {
        public virtual void Reset()
        {
        }

        [UsedImplicitly]
        public class Stat<T> where T : struct
        {
            private T value;
            public void SetValue(T newValue) => value = newValue;
            public T GetValue() => value;
        }
    }
}