using CSM;
using JetBrains.Annotations;
using Playground.States.Player;
using UnityEngine;

namespace Playground.States
{
    [UsedImplicitly]
    [StateDescriptor(group = 3, priority = 5)]
    public class MeleeAttack : State<PlayerStats>
    {
        private int combo;
        private Vector2 axis;
        private PlayerActor player;
        private Animator animator;
        private static readonly int attack = Animator.StringToHash("attack");
        private static readonly int attackSpeed = Animator.StringToHash("attackSpeed");

        public override void Init(Message initiator)
        {
            player = (PlayerActor)actor;
            combo = 0;
            animator = actor.GetComponent<Animator>();                    
            Attack();
        }

        public override void Update()
        {
            stats.Speed *= 0.5f;
            stats.TurnSpeed *= 0.3f;
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("attack"))
                Exit();
        }

        public override bool Process(Message message)
        {
            if (message.phase == Message.Phase.Started)
            {
                if (message.name == "Attack")
                {
                    message.processed = true;
                    combo++;
                    Attack();
                }
            }
            if (message.name == "Move")
            {
                message.processed = true;
                player.axis = message.axis;
            }
            return true; //Stop all states below from processing inputs.
        }

        private void Attack()
        {
            animator.SetFloat(attackSpeed, stats.AttackSpeed);
            animator.SetTrigger(attack);
        }

        public override void End()
        {
            player.axis = axis;
            base.End();
        }
    }
}