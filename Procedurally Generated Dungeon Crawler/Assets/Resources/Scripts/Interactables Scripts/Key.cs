using UnityEngine;

namespace ProceduralDungeon.Items
{
    public class Key : MonoBehaviour
    {
        private bool isCollected = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log($"Key collision detected with: {collision.gameObject.name}");
            if (collision.CompareTag("Player") && !isCollected)
            {
                Debug.Log("Key collected by player!");
                isCollected = true;

                if (ItemManager.Instance != null)
                {
                    ItemManager.Instance.AddKey();
                }
                else
                {
                    Debug.LogError("ItemManager.Instance is null!");
                }
                
                Debug.Log("Key destroyed!");
                Destroy(gameObject);
            }
        }
    }
}