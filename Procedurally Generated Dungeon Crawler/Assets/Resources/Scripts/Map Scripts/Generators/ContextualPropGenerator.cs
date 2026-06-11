using System.Collections.Generic;
using ProceduralDungeon.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProceduralDungeon.Generator
{
    public class ContextualPropGenerator : MonoBehaviour
    {
        private DungeonSettings dungeonSettings;
        private int seed;

        public ContextualPropGenerator(DungeonSettings settings, int randomSeed)
        {
            dungeonSettings = settings;
            seed = randomSeed;
        }

        public void GenerateContextualProps(List<DungeonGenerator.Room> rooms, Tilemap biomeTilemap,
            Tilemap decorationTilemap, BiomeGenerator biomeGen, bool[,] floorData)
        {
            if (!dungeonSettings.EnableContextualProps)
            {
                return;
            }

            System.Random propRNG = new System.Random(seed + 4000);
            int propCount = 0;

            for (int x = 0; x < dungeonSettings.DungeonWidth; x++)
            {
                for (int y = 0; y < dungeonSettings.DungeonHeight; y++)
                {
                    if (!floorData[x, y]) continue;
                    if (decorationTilemap.GetTile(new Vector3Int(x, y, 0)) != null) continue;
                    if (biomeTilemap.GetTile(new Vector3Int(x, y, 0)) != null) continue;

                    int roomIndex = GetRoomAtPos(x, y, rooms);
                    if (roomIndex < 0) continue;

                    BiomeType biome = biomeGen.GetBiome(roomIndex);

                    if (propRNG.NextDouble() < dungeonSettings.ContextualPropChance)
                    {
                        TileBase propTile = null;

                        if (biome == BiomeType.Molten && dungeonSettings.MoltenProps.Length > 0)
                        {
                            propTile = dungeonSettings.MoltenProps[propRNG.Next(0, dungeonSettings.MoltenProps.Length)];
                        }
                        else if (biome == BiomeType.Flooded && dungeonSettings.FloodedProps.Length > 0)
                        {
                            propTile = dungeonSettings.FloodedProps[
                                propRNG.Next(0, dungeonSettings.FloodedProps.Length)];
                        }

                        if (propTile != null)
                        {
                            decorationTilemap.SetTile(new Vector3Int(x, y, 0), propTile);
                            propCount++;
                        }
                    }
                }
            }

            Debug.Log($"Placed {propCount} contextual props");
        }

        private int GetRoomAtPos(int x, int y, List<DungeonGenerator.Room> rooms)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                DungeonGenerator.Room r = rooms[i];

                if (r.IsPointInRoom(x, y))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}