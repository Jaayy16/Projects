using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProceduralDungeon.Map
{
    public class SecretRoomDoor : MonoBehaviour
    {
        [SerializeField] private Vector3 exitPosition;
        private bool isEntryDoor = true;

        public void SetAsEntryDoor()
        {
            isEntryDoor = true;
        }

        public void SetAsExitDoor(Vector3 returnPos)
        {
            isEntryDoor = false;
            exitPosition = returnPos;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                if (isEntryDoor)
                {
                    EnterSecretRoom(collision.gameObject);
                }
                else
                {
                    ExitSecretRoom(collision.gameObject);
                }
            }
        }

        private void EnterSecretRoom(GameObject player)
        {
            Vector3 playerPos = player.transform.position;
            
            SceneManager.LoadScene("SecretRoom");
            
            PlayerPrefs.SetFloat("ReturnPosX", playerPos.x);
            PlayerPrefs.SetFloat("ReturnPosY", playerPos.y);
            PlayerPrefs.SetFloat("ReturnPosZ", playerPos.z);
        }

        private void ExitSecretRoom(GameObject player)
        {
            SceneManager.LoadScene("GameScene");

        }
        
    }
}