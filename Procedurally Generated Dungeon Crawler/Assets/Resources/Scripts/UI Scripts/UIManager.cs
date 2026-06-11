using System;
using ProceduralDungeon.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProceduralDungeon.Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI keysText;
        [SerializeField] private TextMeshProUGUI levelsText;

        private ItemManager itemManager;
        private DifficultyManager difficultyManager;
        private ProceduralDungeon.Player.PlayerController playerController;

        void Start()
        {
            itemManager = ItemManager.Instance;
            difficultyManager = DifficultyManager.Instance;

            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerController = player.GetComponent<ProceduralDungeon.Player.PlayerController>();
            }
        }

        private void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            int keysCollected = 0 + itemManager.GetKeysCollected();
            int levelsCleared = 0 + difficultyManager.GetDungeonLevel();
            float healthNum = 0 + playerController.GetHealth();
            
            if (itemManager != null && keysText != null)
            {
                keysText.text = $"Keys: {keysCollected}";
            }

            if (difficultyManager != null && levelsText != null)
            {
                levelsText.text = $"Levels: {levelsCleared}";
            }

            if (playerController != null)
            {
                float health = playerController.GetHealth();

                if (healthText != null)
                {
                    healthText.text = $"Health: {healthNum}";
                }

            }
        }
    }
}