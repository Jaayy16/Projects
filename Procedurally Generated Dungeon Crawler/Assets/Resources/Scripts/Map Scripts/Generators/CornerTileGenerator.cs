using System.Collections.Generic;
using ProceduralDungeon.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProceduralDungeon.Generator
{
    public class CornerTileGenerator : MonoBehaviour
    {
        private DungeonSettings dungeonSettings;
        public CornerTileGenerator(DungeonSettings settings)
        {
            dungeonSettings = settings;
        }

        public void GenerateCornerTiles(List<DungeonGenerator.Room> rooms, Tilemap wallTilemap)
        {
            if (!dungeonSettings.EnableContextualTiling || dungeonSettings.CornerTile == null)
            {
                return;
            }

            int cornerCount = 0;

            foreach (DungeonGenerator.Room room in rooms)
            {
                int[] xCorners = { room.x, room.x + room.width - 1 };
                int[] yCorners = { room.y, room.y + room.height - 1 };

                foreach (int xCorner in xCorners)
                {
                    foreach (int yCorner in yCorners)
                    {
                        if (!room.IsInRoom(xCorner, yCorner))
                        {
                            Vector3Int pos = new Vector3Int(xCorner, yCorner, 0);

                            if (wallTilemap.GetTile(pos) != null)
                            {
                                wallTilemap.SetTile(pos, dungeonSettings.CornerTile);
                                cornerCount++;
                            }
                        }
                    }
                }
            }
            
            Debug.Log($"Placed  corners: {cornerCount}");
        }
        
    }
}