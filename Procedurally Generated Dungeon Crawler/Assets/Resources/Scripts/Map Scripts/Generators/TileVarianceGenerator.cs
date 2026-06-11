using System;
using ProceduralDungeon.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace ProceduralDungeon.Generator
{
    public class TileVarianceGenerator
    {
        private DungeonSettings dungeonSettings;
        private int seed;

        public TileVarianceGenerator(DungeonSettings dungeonSettings, int seed)
        {
            this.dungeonSettings = dungeonSettings;
            this.seed = seed;
        }

        public float GetNoiseValue(int x, int y)
        {
            float noiseValue = Mathf.PerlinNoise(
                (x + seed * 1000) / dungeonSettings.NoiseScale,
                (y + seed * 1000) / dungeonSettings.NoiseScale);
            
            return noiseValue;
        }

        public TileBase GetFloorVariant(int x, int y, TileBase tile)
        {
            if (!dungeonSettings.EnableFloorVariance || dungeonSettings.FloorVariants == null ||
                dungeonSettings.FloorVariants.Length == 0)
            {
                return tile;
            }

            foreach (TileBase variant in dungeonSettings.FloorVariants)
            {
                if (variant == null)
                {
                    Debug.LogError("Null floor variant tile");
                }
            }
            
            float noise = GetNoiseValue(x, y);

            if (noise > dungeonSettings.VarianceThreshold)
            {
                int varIndex = Mathf.FloorToInt((noise - dungeonSettings.VarianceThreshold) /
                    (1f - dungeonSettings.VarianceThreshold) * dungeonSettings.FloorVariants.Length);
                    
                varIndex= Mathf.Clamp(varIndex, 0, dungeonSettings.FloorVariants.Length - 1);
                
                TileBase variant = dungeonSettings.FloorVariants[varIndex];

                if (x < 5 & y < 5)
                {
                    Debug.Log($"Wall variant at({x}, {y}): {variant.name}");
                }
                
                return variant;
            }
            return tile;
        }

        public TileBase GetWallVariant(int x, int y, TileBase tile)
        {
            if (!dungeonSettings.EnableWallVariance || dungeonSettings.WallVariants == null ||
                dungeonSettings.WallVariants.Length == 0)
            {
                return tile;
            }
            
            float noise = GetNoiseValue(x, y);

            if (noise > dungeonSettings.VarianceThreshold)
            {
                int varIndex = Mathf.FloorToInt((noise - dungeonSettings.VarianceThreshold) /
                    (1f - dungeonSettings.VarianceThreshold) * dungeonSettings.WallVariants.Length);

                varIndex = Mathf.Clamp(varIndex, 0, dungeonSettings.FloorVariants.Length - 1);
                
                TileBase variant = dungeonSettings.WallVariants[varIndex];

                if (x < 5 & y < 5)
                {
                    Debug.Log($"Wall variant at({x}, {y}): {variant.name}");
                }
                
                return variant;
            }
            return tile;
        }
    }
}