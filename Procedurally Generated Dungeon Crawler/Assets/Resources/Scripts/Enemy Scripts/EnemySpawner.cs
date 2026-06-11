using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProceduralDungeon.Enemy
{

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy Prefabs")] 
        [SerializeField] private GameObject meleeEnemyPrefab;
        [SerializeField] private GameObject rangedEnemyPrefab;

        [Header("Spawn Settings")] 
        [SerializeField] private int minEnemiesPerRoom = 1;
        [SerializeField] private int maxEnemiesPerRoom = 5;
        [SerializeField] private float rangedEnemyRatio = 0.4f;
        [SerializeField] private Tilemap floorTilemap;

        private int totalEnemiesSpawned;
        
        public void SpawnEnemiesInRoom(Rect roomBounds, int roomIndex)
        {
            if (!ValidAssignment()) return;
            
            int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
        
            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 spawnPos = GetRandomFloorTileInRoom(roomBounds);
        
                if (spawnPos == Vector3.zero) continue;
        
                bool isRanged = Random.value < rangedEnemyRatio;
                GameObject prefab = isRanged ? rangedEnemyPrefab : meleeEnemyPrefab;
                
                GameObject spawnedEnemy = Instantiate(prefab, spawnPos, Quaternion.identity);

                BaseEnemy enemyComponent = spawnedEnemy.GetComponent<BaseEnemy>();
        
                if (enemyComponent != null)
                {
                    string enemyType = isRanged ? "Ranged" : "Melee";
                    spawnedEnemy.name = $"{enemyType}Enemy_{totalEnemiesSpawned}";
                    
                    totalEnemiesSpawned++;
                }
                else
                {
                    Destroy(spawnedEnemy);
                }
            }
        }

        private Vector3 GetRandomFloorTileInRoom(Rect roomBounds)
        {

            int attemptCount = 0;
            
            for (int attempts = 0; attempts < 50; attempts++)
            {
                int xMin = (int)roomBounds.xMin;
                int xMax = (int)roomBounds.xMax;
                int yMin = (int)roomBounds.yMin;
                int yMax = (int)roomBounds.yMax;
                
                int xRand = Random.Range(xMin + 1, xMax - 1);
                int yRand = Random.Range(yMin + 1, yMax - 1);
                
                Vector3Int cellPos = new Vector3Int(xRand, yRand, 0);
                TileBase tile = floorTilemap.GetTile(cellPos);

                attemptCount++;
                
                if (tile != null) 
                {
                    Vector3 worldPos = floorTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0f);
                    return worldPos;
                }
            }

            BoundsInt cellBounds = floorTilemap.cellBounds;
            int floorTileCount = 0;

            foreach (Vector3Int pos in cellBounds.allPositionsWithin)
            {
                if (floorTilemap.GetTile(pos) != null) floorTileCount++;
            }
            
            return Vector3Int.zero;
        }

        private bool ValidAssignment()
        {
            if(meleeEnemyPrefab == null) return false;
            
            if(rangedEnemyPrefab == null) return false;
            
            if(floorTilemap == null) return false;
            
            return true;
        }

        public int GetTotalEnemiesSpawned()
        {
            return totalEnemiesSpawned;
        }

        public void ResetCount()
        {
            totalEnemiesSpawned = 0;
        }
    }
}