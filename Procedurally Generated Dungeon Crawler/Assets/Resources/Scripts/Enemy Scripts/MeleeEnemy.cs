using ProceduralDungeon.Combat;
using UnityEngine;

namespace ProceduralDungeon.Enemy
{
    public class MeleeEnemy : BaseEnemy
    {
        [Header("Melee Settings")]
        [SerializeField] protected float atkDamage = 15f;
        [SerializeField] protected float atkRange = 0.8f;
        [SerializeField] protected float atkCooldown = 1f;
        [SerializeField] private new float moveSpeed = 5f;
        [SerializeField] private new float maxHealth = 40f;

        
        private float lastAtkTime = 0f;

        protected override void OnPlayerDetected()
        {
            Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            if (playerTransform == null) return;
            
            Vector2 dirToPlayer = (playerTransform.position - this.transform.position).normalized;
            
            RotateTowards(dirToPlayer);
            
            rb.linearVelocity = dirToPlayer * moveSpeed;
            
            float distToPlayer = Vector2.Distance(this.transform.position, playerTransform.position);

            if (distToPlayer < atkRange)
            {
                if (Time.time - lastAtkTime > atkCooldown)
                {
                    AttackPlayer(playerTransform);
                    lastAtkTime = Time.time;
                }
            }
        }

        protected override void OnPlayerNotDetected()
        {
            rb.linearVelocity = Vector2.zero;
        }

        private void AttackPlayer(Transform playerTransform)
        {
            IAttackable playerAttackable = playerTransform.GetComponent<IAttackable>();

            if (playerAttackable != null)
            {
                playerAttackable.TakeDamage(atkDamage);
            }
        }
    }
}