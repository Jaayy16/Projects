using System;
using UnityEngine;

namespace ProceduralDungeon.Items
{
    public class ItemManager : MonoBehaviour
    {
        private static ItemManager instance;

        private int keysCollected = 0;

        private int ammoCount = 0;

        void Awake()
        {
            if (instance != null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddKey()
        {
            keysCollected++;
        }

        public void AddAmmo(int amount)
        {
            ammoCount += amount;
        }

        public void RemoveAmmo(int amount)
        {
            ammoCount -= amount;
            ammoCount = Mathf.Max(0, ammoCount);
        }
        
        public int GetKeysCollected() => keysCollected;
        public int GetAmmoCount() => ammoCount;
        public static ItemManager Instance => instance;

    }
}