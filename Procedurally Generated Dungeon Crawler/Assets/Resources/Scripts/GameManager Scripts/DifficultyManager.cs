using UnityEngine;

namespace ProceduralDungeon.Managers
{
    public class DifficultyManager : MonoBehaviour
    {
       private static DifficultyManager instance;

       private int dungeonLevel = 0;
       private float difficultyMultiplier = 1f;

       [SerializeField] private float difficultyIncreasePerLevel = 0.2f;
       [SerializeField] private float maxDifficultyMultiplier = 5f;

       void Awake()
       {
           if (instance == null)
           {
               instance = this;
               DontDestroyOnLoad(gameObject);
           }
           else
           {
               Destroy(gameObject);
           }
       }

       public void IncrementDungeonLevel()
       {
           dungeonLevel++;
           UpdateDifficultyMultiplier();
           Debug.Log($"Dungeon Level: {dungeonLevel}, Difficulty Multiplier: {difficultyMultiplier:F2}x");
       }

       private void UpdateDifficultyMultiplier()
       {
           difficultyMultiplier = 1f + (dungeonLevel * difficultyIncreasePerLevel);
           difficultyMultiplier = Mathf.Min(difficultyMultiplier, maxDifficultyMultiplier);
       }
  
       public float GetDifficultyMultiplier() => difficultyMultiplier;
       public int GetDungeonLevel() => dungeonLevel;

       public void ResetDifficulty()
       {
           dungeonLevel = 0;
           difficultyMultiplier = 1f;
       }
       
       public static DifficultyManager Instance => instance;
       
    }
}