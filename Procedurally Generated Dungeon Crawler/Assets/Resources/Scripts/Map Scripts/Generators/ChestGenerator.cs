using System.Collections.Generic;
using ProceduralDungeon.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProceduralDungeon.Generator
{
    public class ChestGenerator : MonoBehaviour
    {
        private DungeonSettings dungeonSettings;
        private int Seed;

        public ChestGenerator(DungeonSettings settings, int randomSeed)
        {
            dungeonSettings =  settings;
            Seed = randomSeed;
        }

        public void GenerateChestsInDungeon(List<DungeonGenerator.Room> rooms, Tilemap floorTilemap,
            GameObject chestPrefab, bool[,] floorData)
        {
            if (chestPrefab == null)
            {
                return;
            }
            
            System.Random chestRNG = new System.Random(Seed + 3000);
            int chestCount = 0;

            for (int i = 1; i < rooms.Count; i++)
            {
                DungeonGenerator.Room room = rooms[i];

                int chestsInRoom = chestRNG.Next(1, 3);

                for (int j = 0; j < chestsInRoom; j++)
                {
                    Vector3 chestPos = GetRandomFloorTileInRoom(room, floorTilemap, chestRNG);

                    if (chestPos != Vector3.zero)
                    {
                        Instantiate(chestPrefab, chestPos, Quaternion.identity);
                        chestCount++;
                    }
                }
            }
        }

        private Vector3 GetRandomFloorTileInRoom(DungeonGenerator.Room room, Tilemap floorTilemap,
            System.Random rng)
        {
            for (int attempts = 0; attempts < 50; attempts++)
            {
                int xRand = rng.Next((int)room.x + 1, (int)room.x + room.width - 1);
                int yRand = rng.Next((int)room.y + 1, (int)room.y + room.height - 1);
                
                Vector3Int cellPos = new Vector3Int(xRand, yRand, 0);
                TileBase tile = floorTilemap.GetTile(cellPos);

                if (tile != null)
                {
                    return floorTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0f);
                }
            }

            return Vector3.zero;
        }
        
    }
}