using System;
using ProceduralDungeon.Generator;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProceduralDungeon.Map
{
    public class SecretRoomTrigger : MonoBehaviour
    {
        [SerializeField] private Tilemap decorationTilemap;
        [SerializeField] private GameObject secretDoorPrefab;
        private SecretRoomGenerator secretRoomGenerator;

        public void SetSecretRoomGenerator(SecretRoomGenerator generator)
        {
            secretRoomGenerator = generator;
        }

        void Update()
        {
            if (secretRoomGenerator == null || decorationTilemap == null) return;
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            Vector3Int playerCell = decorationTilemap.WorldToCell(player.transform.position);
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    Vector3Int checkPos = playerCell + new Vector3Int(x, y, 0);

                    if (secretRoomGenerator.IsSecretRoomEntrance(checkPos))
                    {
                        TileBase tile = decorationTilemap.GetTile(checkPos);

                        if (tile != null && tile.name == "SecretRoomDoor")
                        {
                            secretRoomGenerator.DiscoverSecretRoom(checkPos);
                        }
                    }
                }
            }
        }
    }
}