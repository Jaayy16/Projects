using System.Collections.Generic;
using ProceduralDungeon.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace ProceduralDungeon.Generator
{
    public enum BiomeType
    {
        Normal,
        Flooded,
        Molten
    }

    public enum VegetationType
    {
        None,
        Moss,
        Vines
    }
    
    public class BiomeGenerator
    {
        private DungeonSettings dungeonSettings;
        private int seed;
        private Dictionary<int, BiomeType> roomBiomeTypes;
        
        public BiomeGenerator(DungeonSettings settings, int randomSeed)
        {
            dungeonSettings = settings;
            seed = randomSeed;
            roomBiomeTypes = new Dictionary<int, BiomeType>();
        }

        public void AssignBiome(int roomCount)
        {
            System.Random random = new System.Random(seed + 500);

            for (int i = 0; i < roomCount; i++)
            {
                if (i == 0 || i == roomCount - 1)
                {
                    roomBiomeTypes[i] = BiomeType.Normal;
                }
                else
                {
                    int biomeChoice = random.Next(0,3);
                    roomBiomeTypes[i] = (BiomeType)biomeChoice;
                    Debug.Log($"Room {i} Assigned: {roomBiomeTypes[i]} biome");
                }
            }
        }

        public BiomeType GetBiome(int roomIndex)
        {
            if (roomBiomeTypes.ContainsKey(roomIndex))
            {
                return roomBiomeTypes[roomIndex];
            }
            
            return BiomeType.Normal;
        }

        public void GenerateRoomBiome(BiomeType biomeType, Tilemap floorTileMap, Tilemap biomeTileMap, int roomX,
            int roomY, int roomW, int roomH, bool isSquareRoom)
        {

            if (biomeType == BiomeType.Normal)
            {
                return;
            }    
            
            System.Random biomeRand = new System.Random(seed + roomX * 1000 + roomY * 2000);

            if (biomeType == BiomeType.Flooded)
            {
                GenerateFloodedBiome(floorTileMap, biomeTileMap, roomX, roomY, roomW, roomH, biomeRand, isSquareRoom);
            }
            else if (biomeType == BiomeType.Molten)
            {
                GenerateMoltenBiome(floorTileMap, biomeTileMap, roomX, roomY, roomW, roomH, biomeRand, isSquareRoom);
            }
        }

        private bool isPointInRoom(int x, int y, int roomX, int roomY, int roomW, int roomH, bool isSquareRoom)
        {
            return x > roomX && x < roomX + roomW - 1 && y > roomY && y < roomY + roomH - 1;
        }

        private void GenerateFloodedBiome(Tilemap floorTileMap, Tilemap biomeTileMap, int roomX, int roomY,
            int roomW, int roomH, System.Random random, bool isSquareRoom)
        {
            if (dungeonSettings.FloodedTiles == null || dungeonSettings.FloodedTiles.Length == 0)
            {
                return;
            }

            float floodChance = dungeonSettings.FloodChance;

            for (int x = roomX; x < roomX + roomW; x++)
            {
                for (int y = roomY; y < roomY + roomH; y++)
                {
                    if (biomeTileMap.GetTile(new Vector3Int(x, y, 0)) != null)
                    {
                        continue;
                    }
                    
                    if (floorTileMap.GetTile(new Vector3Int(x, y, 0)) != null && random.NextDouble() < floodChance)
                    {
                        TileBase floodTile =
                            dungeonSettings.FloodedTiles[random.Next(0, dungeonSettings.FloodedTiles.Length)];
                        biomeTileMap.SetTile(new Vector3Int(x, y, 0), floodTile);
                    }        
                }
            }
            
            Debug.Log($"Flooded biome at ({roomX}, {roomY})");
        }

        private void GenerateMoltenBiome(Tilemap floorTileMap, Tilemap biomeTileMap, int roomX, int roomY, int roomW,
            int roomH, System.Random random, bool isSquareRoom)
        {
            if (dungeonSettings.MoltenTiles == null || dungeonSettings.MoltenTiles.Length == 0)
            {
                return;
            }

            float moltenChance = dungeonSettings.MoltenChance;

            for (int x = roomX; x < roomX + roomW; x++)
            {
                for (int y = roomY; y < roomY + roomH; y++)
                {
                    if (biomeTileMap.GetTile(new Vector3Int(x, y, 0)) != null)
                    {
                        continue;
                    }
                    
                    if (floorTileMap.GetTile(new Vector3Int(x, y, 0)) != null && random.NextDouble() < moltenChance)
                    {
                        TileBase moltenTile =
                            dungeonSettings.MoltenTiles[random.Next(0, dungeonSettings.MoltenTiles.Length)];
                        biomeTileMap.SetTile(new Vector3Int(x, y, 0), moltenTile);
                    }        
                }
            }
            
            Debug.Log($"Molten biome at ({roomX}, {roomY})");        
        }

        public void BlendCorridorBiome(BiomeType biome1, BiomeType biome2, Tilemap biomeTileMap, int startX,
            int endX, int startY, int endY, System.Random random)
        {
            if (biome1 == BiomeType.Normal && biome2 == BiomeType.Normal)
            {
                return;
            }

            BiomeType biomeBlend = biome1 != BiomeType.Normal ? biome1 : biome2;
            float blendChance = 0.3f;

            for (int x = startX; x < endX; x++)
            {
                if (random.NextDouble() < blendChance)
                {
                    TileBase tile = GetBiomeTile(biomeBlend, random);
                    if (tile != null && biomeTileMap.GetTile(new Vector3Int(x, startY, 0)) == null)
                    {
                        biomeTileMap.SetTile(new Vector3Int(x, startY, 0), tile);
                    }
                }
            }

            for (int y = startY; y < endY; y++)
            {
                if (random.NextDouble() < blendChance)
                {
                    TileBase tile = GetBiomeTile(biomeBlend, random);
                    if (tile != null && biomeTileMap.GetTile(new Vector3Int(endX, y, 0)) == null)
                    {
                        biomeTileMap.SetTile(new Vector3Int(startX, y, 0), tile);
                    }
                }
            }
        }

        private TileBase GetBiomeTile(BiomeType biomeType, System.Random random)
        {
            if (biomeType == BiomeType.Flooded && dungeonSettings.FloodedTiles.Length > 0)
            {
                return dungeonSettings.FloodedTiles[random.Next(0, dungeonSettings.FloodedTiles.Length)];
            }
            else if (biomeType == BiomeType.Molten && dungeonSettings.MoltenTiles.Length > 0)
            {
                return dungeonSettings.MoltenTiles[random.Next(0, dungeonSettings.MoltenTiles.Length)];
            }
            
            return null;
        }
    }
}