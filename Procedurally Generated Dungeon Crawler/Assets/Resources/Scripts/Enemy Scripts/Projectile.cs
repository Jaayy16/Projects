using System;
using ProceduralDungeon.Combat;
using UnityEngine;

namespace ProceduralDungeon.Projectile
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 4f;
        [SerializeField] private float speed = 10f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private string targetTag = "Enemy";
        
        private Vector2 direction;
        private float timeAlive = 0f;
        private Rigidbody2D rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        private void Update()
        {
            timeAlive += Time.deltaTime;

            if (timeAlive > lifeTime)
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(Vector2 Shootdir, float shootSpd, float dmg, string targetTagName)
        {
            direction =  Shootdir;
            speed = shootSpd;
            damage = dmg;
            targetTag = targetTagName;
            timeAlive = 0f;
            
            rb.linearVelocity = direction * speed;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(targetTag))
            {
                IAttackable attackable = other.GetComponent<IAttackable>();

                if (attackable != null)
                {
                    attackable.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                Destroy(gameObject);
            } 
        }
    }
}
