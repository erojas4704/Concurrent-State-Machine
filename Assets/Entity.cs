using UnityEngine;

namespace CSM.Entities
{
    public class Entity : Actor
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

            string stackedStates = "";
            foreach(State s in states)
            {
                stackedStates += s + "\n";
            }
            Debug.Log("THERE WAS A STATE CHANGE");
            Debug.Log(stackedStates);

            foreach (State state in states)
            {
                if (state.GetType().IsSubclassOf(typeof(EntityState)))
                {
                    EntityState e = (EntityState)state;
                    lastCalculatedStat = e.Reduce(this, lastCalculatedStat);
                    e.stats = lastCalculatedStat;
                    Debug.Log($"Processing state {e.GetType()}: " +
                        $"EState stats {e.stats} Last calculated stats {lastCalculatedStat} base stats {this.stats}");
                }
            }

            finalStats = lastCalculatedStat;
        }
    }
}