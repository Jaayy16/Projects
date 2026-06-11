using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProceduralDungeon.Generator
{
    public class VegetationRenderer : MonoBehaviour
    {
        [SerializeField] private Tilemap decorationTilemap;
        
        [SerializeField] TileBase[] mossTiles = new TileBase[3];
        [SerializeField] TileBase[] vineTiles = new TileBase[3];
        
        private VegitationGenerator vegetationGenerator;

        public void SetVegetationGenerator(VegitationGenerator vegetationGen)
        {
            vegetationGenerator = vegetationGen;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (vegetationGenerator == null) return;

            List<VegitationData> vegs = vegetationGenerator.GetVegetationTiles();

            foreach (VegitationData veg in vegs)
            {
                TileBase tile = GetTileForVegetation(veg);

                if (tile != null)
                {
                    decorationTilemap.SetTile(veg.position, tile);
                }
            }
        }

        private TileBase GetTileForVegetation(VegitationData veg)
        {
            TileBase[] tileSet = veg.vegitationType switch
            {
                VegetationType.Moss => mossTiles,
                VegetationType.Vines => vineTiles,
                _ => null
            };
            
            if(tileSet == null || tileSet.Length == 0) return null;
            
            int tileIndex = Mathf.Min((int)(veg.growthStage * tileSet.Length), tileSet.Length - 1);
            return tileSet[tileIndex];
        }
    }
}