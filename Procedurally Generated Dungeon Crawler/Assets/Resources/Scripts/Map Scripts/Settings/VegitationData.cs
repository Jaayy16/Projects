using UnityEngine;

namespace ProceduralDungeon.Generator
{
    [System.Serializable]
    public class VegitationData : MonoBehaviour
    {
        public Vector3Int position;
        public VegetationType vegitationType;
        public float growthStage;
        public float spreadCooldown;

        public VegitationData(Vector3Int pos, VegetationType type)
        {
            position = pos;
            vegitationType = type;
            growthStage = 0.5f;
            spreadCooldown = 0f;
        }

    }
}