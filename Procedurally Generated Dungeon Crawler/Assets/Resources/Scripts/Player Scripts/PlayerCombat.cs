using ProceduralDungeon.Combat;
using UnityEngine;

namespace ProceduralDungeon.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float detectionRadius = 6f; 
        [SerializeField] private LayerMask enemyLayer;
        
        [Header("Combat")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private float projectileSpeed = 15f;
        [SerializeField] private float projectileDamage = 10f;
        [SerializeField] private float atkCooldown = 0.5f;
        [SerializeField] private float rotationSpeed = 10f;

        private float lastAtkTime = 0f;
        private IAttackable lockedEnemy;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (shootPoint == null)
            {
                shootPoint = transform;
            }

            lastAtkTime = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            lockedEnemy = FindClosestEnemy();
            
            if (lockedEnemy != null)
            {
                RotateTowardsEnemy(lockedEnemy);
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryShoot();
            }
        }

        private IAttackable FindClosestEnemy()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
            
            if(hits.Length == 0) return null;

            IAttackable closestEnemy = null;
            float closestDist = float.MaxValue;

            foreach (Collider2D hit in hits)
            {
                IAttackable attackable = hit.GetComponent<IAttackable>();

                if (attackable != null && attackable.IsAlive())
                {
                    float dist = Vector2.Distance(transform.position, attackable.GetTransform().position);

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestEnemy = attackable;
                    }
                }
            }
            return closestEnemy;
        }

        private void RotateTowardsEnemy(IAttackable enemy)
        {
            Vector2 dirToEnemy = (enemy.GetTransform().position - transform.position).normalized;
            float angle = Mathf.Atan2(dirToEnemy.y, dirToEnemy.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward),
                Time.deltaTime * rotationSpeed);
        }

        private void TryShoot()
        {
            if (Time.time - lastAtkTime < atkCooldown) return;

            if (lockedEnemy != null && lockedEnemy.IsAlive())
            {
                ShootAtEnemy(lockedEnemy);
                lastAtkTime = Time.time;
            }
        }

        private void ShootAtEnemy(IAttackable enemy)
        {
            if (projectilePrefab == null)
            {
                return;
            }
            
            Vector2 shootDir = (enemy.GetTransform().position - shootPoint.position).normalized;
            
            GameObject projectileObj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            Projectile.Projectile projectile = projectileObj.GetComponent<Projectile.Projectile>();

            if (projectile != null)
            {
                projectile.Initialize(shootDir, projectileSpeed, projectileDamage, "Enemy");

            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
        
    }
}
