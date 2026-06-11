using System.Collections.Generic;
using ProceduralDungeon.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProceduralDungeon.Generator
{
    public class VegitationGenerator : MonoBehaviour
    {
        private DungeonSettings dungeonSettings;
        public int seed;
        private List<VegitationData> vegetationTiles = new List<VegitationData>();
        private Tilemap decorationTilemap;
        private BiomeGenerator biomeGenerator;

        public VegitationGenerator(DungeonSettings settings, int randomSeed, BiomeGenerator biomeGen)
        {
            dungeonSettings = settings;
            seed = randomSeed;
            biomeGenerator = biomeGen;
        }

        public void InitializeVeg(List<DungeonGenerator.Room> rooms, Tilemap decorTilemap, Tilemap biomeTilemap,
            bool[,] floorData)
        {
            decorationTilemap = decorTilemap;
            vegetationTiles.Clear();

            System.Random vegRNG = new System.Random(seed + 5000);
            int vegCount = 0;

            for (int i = 0; i < rooms.Count; i++)
            {
                DungeonGenerator.Room room = rooms[i];
                BiomeType biome = biomeGenerator.GetBiome(i);
                VegetationType vegType = GetVegForBiome(biome);

                if (vegType == VegetationType.None) continue;

                int vegInRoom = vegRNG.Next(vegCount);

                for (int j = 0; j < vegInRoom; j++)
                {
                    int maxAttempts = 20;
                    bool placed = false;

                    int x = vegRNG.Next(room.x + 1, room.x + room.width - 1);
                    int y = vegRNG.Next(room.y + 1, room.y + room.height - 1);

                    Vector3Int pos = new Vector3Int(x, y, 0);

                    if (floorData[x, y] && decorationTilemap.GetTile(pos) == null && biomeTilemap.GetTile(pos) == null)
                    {
                        VegitationData veg = new VegitationData(pos, vegType);
                        vegetationTiles.Add(veg);
                        placed = true;
                        vegCount++;
                        break;
                    }
                }
            }
        }

        public void UpdateVeg(float deltaTime, Tilemap decorTilemap, bool[,] floorData)
        {
            if (vegetationTiles.Count == 0) return;
            
            System.Random updateRNG = new System.Random(seed + 6000 + (int)(Time.time * 100));

            for (int i = vegetationTiles.Count - 1; i >= 0; i--)
            {
                VegitationData veg = vegetationTiles[i];
                
                veg.growthStage = Mathf.Min(veg.growthStage + deltaTime * 0.1f, 1f);

                veg.spreadCooldown -= deltaTime;

                if (veg.growthStage > 0.7f && veg.spreadCooldown <= 0f)
                {
                    if (updateRNG.NextDouble() < 0.3f)
                    {
                        AttemptSpread(veg, updateRNG, decorTilemap, floorData);
                        veg.spreadCooldown = 5f;
                    }
                }
            }
        }

        private void AttemptSpread(VegitationData parentVeg, System.Random updateRNG, Tilemap decorTilemap,
            bool[,] floorData)
        {
            int[] offsets = { -1, 0, 1 };
            List<Vector3Int> validSpots = new List<Vector3Int>();

            foreach (int xOff in offsets)
            {
                foreach (int yOff in offsets)
                {
                    if (xOff == 0 && yOff == 0) continue;

                    int newX = parentVeg.position.x + xOff;
                    int newY = parentVeg.position.y + yOff;

                    Vector3Int newPos = new Vector3Int(newX, newY, 0);

                    if (floorData[newX, newY] && decorTilemap.GetTile(newPos) == null)
                    {
                        validSpots.Add(newPos);
                    }
                }
            }

            if (validSpots.Count > 0)
            {
                Vector3Int chosenSpot = validSpots[updateRNG.Next(validSpots.Count)];
                VegitationData newVeg = new VegitationData(chosenSpot, parentVeg.vegitationType);
                newVeg.growthStage = 0.3f;
                vegetationTiles.Add(newVeg);
            }
        }

        private VegetationType GetVegForBiome(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Normal => VegetationType.None,
                BiomeType.Flooded => VegetationType.Vines,
                BiomeType.Molten => VegetationType.Moss,
                _ => VegetationType.None
            };
        }
        
        public List<VegitationData> GetVegetationTiles() => vegetationTiles;
        public int GetVegCount() => vegetationTiles.Count;
    }
}