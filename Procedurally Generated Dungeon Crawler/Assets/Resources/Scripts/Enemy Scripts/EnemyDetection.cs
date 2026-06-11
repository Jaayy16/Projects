using UnityEngine;

namespace ProceduralDungeon.Combat
{
    public class EnemyDetection : MonoBehaviour
    {
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private LayerMask playerLayer;
        
        private CircleCollider2D detectCollider;
        private Transform playerTransform;
        private bool isPlayerDetected = false;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            detectCollider = gameObject.AddComponent<CircleCollider2D>();
            detectCollider.radius = detectionRadius;
            detectCollider.isTrigger = true;

            FindPlayer();
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        // Update is called once per frame
        void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
            }

            if (playerTransform != null)
            {
                float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                isPlayerDetected = distToPlayer <= detectionRadius;
            }
            else
            {
                isPlayerDetected = false;
            }
        }
        
        public bool IsPlayerDetected() => isPlayerDetected;
        public Transform GetPlayerTransform() => playerTransform;
        public float GetDetectRadius() => detectCollider.radius;
    }
}