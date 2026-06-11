using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProceduralDungeon.Generator
{
    [System.Serializable]
    public class SecretRoomData
    {
        public Vector3Int entrancePos;
        public bool isDiscovered = false;
        public TileBase entranceTile;

        public SecretRoomData(Vector3Int pos, TileBase tile)
        {
            entrancePos = pos;
            entranceTile = tile;
        }
    }
}