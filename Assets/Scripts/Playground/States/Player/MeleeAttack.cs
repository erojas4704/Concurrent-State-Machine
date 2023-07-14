using CSM;
using JetBrains.Annotations;
using UnityEngine;

namespace Playground.States.Player
{
    [UsedImplicitly]
    [StateDescriptor(group = 3, priority = 5)]
    public class MeleeAttack : State<Playground.PlayerStats>
    {
        private int combo;
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
            if (Timer > 0.15f)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                    Exit();
            }
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
                //Allow passthrough so we can make minor movements.
                return false;
            }

            return true; //Stop all states below from processing inputs.
        }

        private void Attack()
        {
            animator.SetFloat(attackSpeed, stats.AttackSpeed);
            animator.SetTrigger(attack);
        }

    }
}