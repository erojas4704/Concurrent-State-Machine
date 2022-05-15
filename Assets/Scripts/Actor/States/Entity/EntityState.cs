using UnityEngine;
namespace CSM.Entities
{
    public class EntityState : State
    {
        public Entity entity;
        public Stats stats;

        public virtual void Init(Entity entity) { }

        public virtual void Init(Entity entity, Action initiator) { Init(entity); }

        public virtual void Update(Entity entity) { }

        public virtual void Process(Entity entity, Action action)
        {
            Next(entity, action);
        }

        public virtual void End(Entity entity) { }

        public virtual Stats Reduce(Entity entity, Stats stats)
        {
            return stats;
        }

        public override void Init(Actor actor)
        {
            Init((Entity)actor);
        }

        public override void Init(Actor actor, Action initiator)
        {
            Init((Entity)actor, initiator);
        }

        public override void Update(Actor actor)
        {
            Update((Entity)actor);
        }

        public override void Process(Actor actor, Action action)
        {
            Process((Entity)actor, action);
        }

        public override void End(Actor actor)
        {
            End((Entity)actor);
        }
    }
}