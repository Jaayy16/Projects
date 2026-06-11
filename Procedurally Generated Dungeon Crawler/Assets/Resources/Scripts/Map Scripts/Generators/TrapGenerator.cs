using System.Collections.Generic;
using ProceduralDungeon.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProceduralDungeon.Generator
{
    public class TrapGenerator
    {
        private DungeonSettings dungeonSettings;

        private int seed;

        public TrapGenerator(DungeonSettings settings, int randSeed)
        {
            dungeonSettings = settings;
            seed = randSeed;
        }

        public void GenerateTraps(List<DungeonGenerator.Room> rooms, Tilemap floorTilemap, Tilemap trapTilemap,
            bool[,] floorData)
        {
            if (!dungeonSettings.EnableTraps || dungeonSettings.TrapTiles.Length == 0 ||
                dungeonSettings.TrapTiles == null)
            {
                return;
            }
            
            System.Random trapRNG = new System.Random(seed + 2000);

            int trapCount = 0;

            for (int x = 0; x < dungeonSettings.DungeonWidth; x++)
            {
                for (int y = 0; y < dungeonSettings.DungeonHeight; y++)
                {
                    if (!floorData[x, y]) continue;
                    
                    if(trapTilemap.GetTile(new Vector3Int(x, y, 0)) != null) continue;

                    if (trapRNG.NextDouble() < dungeonSettings.TrapChance)
                    {
                        TileBase trapTile = dungeonSettings.TrapTiles[trapRNG.Next(0, dungeonSettings.TrapTiles.Length)];
                        trapTilemap.SetTile(new Vector3Int(x, y, 0), trapTile);
                        trapCount++;
                    }
                }
            }
        }
    }
}
