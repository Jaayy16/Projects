using ProceduralDungeon.Combat;
using UnityEngine;

namespace ProceduralDungeon.Enemy
{
    public abstract class BaseEnemy : MonoBehaviour, IAttackable
    {
        [Header("Base Stats")]
        [SerializeField] protected float maxHealth = 40f;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected float damage = 4f;
        
        [SerializeField] private GameObject keyPrefab;
        [SerializeField] private float keyDropChance = 0.3f;

        private float scaledHealth;
        private float scaledSpeed;
        private float scaledDamage;
        
        protected float currentHealth;
        protected Rigidbody2D rb;
        protected EnemyDetection detection;
        protected bool isAlive = true;

        protected virtual void OnEnable()
        {
            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            detection = GetComponent<EnemyDetection>();

            if (detection == null)
            {
                detection = gameObject.AddComponent<EnemyDetection>();
            }
            
            currentHealth = maxHealth;

            ApplyDifficultyScaling();
        }

        // Update is called once per frame
        void Update()
        {
            if (!isAlive) return;

            if (detection.IsPlayerDetected())
            {
                OnPlayerDetected();
            }
            else
            {
                OnPlayerNotDetected();
            }
        }

        private void ApplyDifficultyScaling()
        {
            ProceduralDungeon.Managers.DifficultyManager difficultyMgr =
                ProceduralDungeon.Managers.DifficultyManager.Instance;

            if (difficultyMgr != null)
            {
                float multiplier = difficultyMgr.GetDifficultyMultiplier();
                
                maxHealth = maxHealth * multiplier;
                currentHealth = maxHealth;
                damage = damage * multiplier;
                moveSpeed  = moveSpeed * multiplier;
            }
        }
        
        protected abstract void OnPlayerDetected();
        
        protected abstract void OnPlayerNotDetected();
        
        protected void RotateTowards(Vector2 direction)
        {
            if (direction.magnitude < 0.01f) return;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        public virtual void TakeDamage(float damage)
        {
            if(!isAlive) return;
            
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        public virtual float GetHealth() => currentHealth;
        
        public virtual Transform GetTransform() => transform;
        
        public virtual bool IsAlive() => isAlive;

        protected virtual void Die()
        {
            isAlive = false;

            if (Random.value < keyDropChance && keyPrefab != null)
            {
                GameObject key = Instantiate(keyPrefab, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }
}
