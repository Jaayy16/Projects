using ProceduralDungeon.Enemy;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProceduralDungeon.Enemy
{
    public class RangedEnemy : BaseEnemy
    {
        [Header("Ranged Attack Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private float shotCooldown = 1.5f;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private float projectileDamage = 8f;
        [SerializeField] private float preferredDistance = 5f;
        [SerializeField] private new float moveSpeed = 3f;
        [SerializeField] private new float maxHealth = 20f;

        private float lastShotTime = 0f;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (shootPoint == null)
            {
                shootPoint = transform;
            }
            
            lastShotTime = Time.time;
        }

        protected override void OnPlayerDetected()
        {
            Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            if (playerTransform == null) return;
            
            Vector2 dirToPlayer = (playerTransform.position - this.transform.position).normalized;
            float distanceToPlayer = Vector2.Distance(this.transform.position, playerTransform.position);
            
            RotateTowards(dirToPlayer);

            if (distanceToPlayer < preferredDistance)
            {
                rb.linearVelocity = -dirToPlayer * moveSpeed;
            }
            else if (distanceToPlayer > preferredDistance + 1f)
            {
                rb.linearVelocity = dirToPlayer * moveSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (Time.time - lastShotTime >= shotCooldown)
            {
                ShootAtPlayer(playerTransform);
                lastShotTime = Time.time;
            }
        }

        protected override void OnPlayerNotDetected()
        {
            rb.linearVelocity = Vector2.zero;
        }

        private void ShootAtPlayer(Transform playerTransform)
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning("No projectile prefab assigned to Ranged Enemy!");
                return;
            }
            
            Vector2 shootDirection =(playerTransform.position - shootPoint.position).normalized;
            
            GameObject projectileObj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            Projectile.Projectile projectile = projectileObj.GetComponent<Projectile.Projectile>();

            if (projectile != null)
            {
                projectile.Initialize(shootDirection, projectileSpeed, projectileDamage, "Player");
            }
            else
            {
                Debug.LogError("[Ranged] projectile prefab missing projectile component!");
            }
        }
    }
}