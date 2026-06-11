using System;
using UnityEngine;
using ProceduralDungeon.Items;

namespace ProceduralDungeon.Items
{
    public class Chest : MonoBehaviour
    {
        [SerializeField] private bool isOpen = false;
        [SerializeField] private float interactionRange = 0.5f;
        
        [Header("Chest Rewards")]
        [SerializeField] private int ammoReward = 20;
        [SerializeField] private float medKitHealAmount = 50f;
        
        private Transform playerTransform;
        private bool playerInRange = false;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (isOpen || playerTransform == null) return;
            
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            playerInRange = distToPlayer <= interactionRange;

            if (playerInRange && Input.GetKeyDown(KeyCode.E))
            {
                OpenChest();
            }
        }

        private void OpenChest()
        {
            isOpen = true;
            
            if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
            {
                GiveAmmoReward();
            }
            else
            {
                GiveMedKitReward();
            }
            
            Destroy(gameObject, 0.5f);
            
        }

        private void GiveAmmoReward()
        {
            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.AddAmmo(ammoReward);
            }
        }

        private void GiveMedKitReward()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                ProceduralDungeon.Player.PlayerController playerController =
                    player.GetComponent<ProceduralDungeon.Player.PlayerController>();

                if (playerController != null)
                {
                    playerController.Heal(medKitHealAmount);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}