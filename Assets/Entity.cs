using UnityEngine;

namespace CSM.Entities
{
    public class Entity : Actor
    {
        public Vector3 velocity;
        public Stats stats;

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

        public void CalculateStats()
        {
            Debug.Log("CALCLUTTE STATS");
            Stats lastStat = this.stats;

            foreach (State state in statePool)
            {
                Debug.Log("Last stats: " + lastStat);
                Debug.Log($"{state.GetType()} {typeof(EntityState)} and {state.GetType().IsSubclassOf(typeof(EntityState))}");
                if (state.GetType().IsSubclassOf(typeof(EntityState)))
                {
                    EntityState e = (EntityState)state;
                    lastStat = e.Reduce(this, lastStat);
                    e.stats = lastStat;
                }
            }
        }
    }
}