using UnityEngine;

namespace CSM.Entity
{
    public class Entity : Actor, ISerializationCallbackReceiver
    {
        public Vector3 velocity;
        public Stats stats;
        public Stats finalStats;

        void Start()
        {
            OnStateChange += OnStateChangeHandler;
            stats.speed = 5f;
        }

        public void OnStateChangeHandler(Actor actor)
        {
            CalculateStats();
        }

        public override void Update()
        {
            base.Update();
        }

        //TODO allow to insert this middleware in and consolidate loops for performance reasons.
        public void CalculateStats()
        {
            Stats lastCalculatedStat = this.stats;
            foreach (State state in states)
            {
                if (state.GetType().IsSubclassOf(typeof(EntityState)))
                {
                    EntityState e = (EntityState)state;
                    lastCalculatedStat = e.Reduce(this, lastCalculatedStat);
                    e.stats = lastCalculatedStat;
                }
            }

            finalStats = lastCalculatedStat;
        }

        #region ISerializationCallbackReceiver implementation
        public new void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
        }
        public new void OnAfterDeserialize()//
        {
            OnStateChange += OnStateChangeHandler;
            base.OnAfterDeserialize();
        }
        #endregion
    }
}