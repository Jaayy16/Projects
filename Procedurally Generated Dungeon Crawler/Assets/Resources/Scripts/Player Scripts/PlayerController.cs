using System;
using System.Collections.Generic;
using ProceduralDungeon.Combat;
using ProceduralDungeon.Generator;
using ProceduralDungeon.Settings;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace ProceduralDungeon.Player
{
    public class PlayerController : MonoBehaviour, IAttackable
    {
        [SerializeField] private DungeonGenerator dungeonGenerator;
        [SerializeField] private DungeonSettings dungeonSettings;
        [SerializeField] private GameManager gameManager;
        
        [Header("Movement Settings")] [SerializeField, Range(0, 20)]
        private float movementSpeed = 5f;

        [SerializeField] private float waterSlowMultiplier = 0.25f;

        [Header("TileMaps")] [SerializeField] private Tilemap wallTilemap;
        [SerializeField] private Tilemap floorTilemap;
        [SerializeField] private Tilemap decorationTilemap;
        [SerializeField] private Tilemap portalTilemap;
        [SerializeField] private Tilemap biomeTileMap;
        [SerializeField] private Tilemap trapTileMap;

        [Header("Trap Detection")] [SerializeField]
        private TileBase[] trapTiles;

        private float lastTrapTriggerTime = 0f;

        [Header("Object To Detect")] [SerializeField]
        private TileBase[] waterTiles;

        [SerializeField] private TileBase[] lavaTiles;

        [Header("Lava Settings")] 
        [SerializeField] private float lavaDmgPerSecond = 10f;
        [SerializeField] private float lavaSlowFactor = 0.25f;

        [Header("Water Settings")] 
        [SerializeField] private float waterSlowFactor = 0.7f;
        
        [SerializeField] private float gridCellSize = 1f;

        private Vector2 inputDirection;
        private Rigidbody2D rb;
        private float currentSpeed;

        private bool isOnWater = false;
        private bool isOnLava = false;
        private float lavaDmgCooldown = 0f;
        
        private BiomeType currentBiome = BiomeType.Normal;

        private float playerHealth = 100f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }

            if (wallTilemap == null)
            {
                wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
            }

            if (floorTilemap == null)
            {
                floorTilemap = GameObject.Find("Floor").GetComponent<Tilemap>();
            }

            if (decorationTilemap == null)
            {
                decorationTilemap = GameObject.Find("Decorations").GetComponent<Tilemap>();
            }

            if (biomeTileMap == null)
            {
                biomeTileMap = GameObject.Find("Biomes").GetComponent<Tilemap>();
            }

            if (trapTileMap == null)
            {
                trapTileMap = GameObject.Find("Traps").GetComponent<Tilemap>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            inputDirection = GetInputMovement();
            CheckIfOnExit();
        }

        private void FixedUpdate()
        {
            if (rb == null) return;

            if (isOnLava) movementSpeed *= lavaSlowFactor;
            if (isOnWater) movementSpeed *= waterSlowFactor;
            
            Vector2 dir = rb.position + inputDirection * movementSpeed * Time.fixedDeltaTime;
            
            if (CanMoveTo(dir))
            {
                rb.linearVelocity = inputDirection * GetCurrentSpeed();
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }

            CheckWaterStatus();
            CheckLavaStatus();
            ApplyLavaDamage();
            UpdateBiomeStatus();

            CheckTrapStatus();
        }

        //Movements functions
        private Vector2 GetInputMovement()
        {
            Vector2 input = Vector2.zero;

            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            return input.normalized;
        }

        private float GetCurrentSpeed()
        {
            return isOnWater ? movementSpeed * waterSlowMultiplier : movementSpeed;
        }

        private bool CanMoveTo(Vector2 direction)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(direction, 0.3f);

            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject.layer == LayerMask.NameToLayer("Wall"))
                {
                    return false;
                }
            }

            return true;
        }

        //Checks if player is able to leave the current dungeon
        private void CheckIfOnExit()
        {
            if (portalTilemap == null) return;

            Vector3Int playerCell = portalTilemap.WorldToCell(transform.position);
            TileBase tile = portalTilemap.GetTile(playerCell);

            if (tile != null && tile.name == "Dungeon Tileset_199")
            {
                OnExitReached(tile);
            }
        }

        private void OnExitReached(TileBase tile)
        {
              GameManager gameManager = FindObjectOfType<GameManager>();

              if (gameManager != null)
              {
                  gameManager.GenerateNewDungeon();
              }
              else
              {
                  Debug.Log("No GameManager found");
              }
        }
        
        //Applies water slowness
        private void CheckWaterStatus()
        {
            Vector3Int playerCell = biomeTileMap.WorldToCell(transform.position);
            isOnWater = false;

            if (biomeTileMap != null)
            {
                TileBase tile = biomeTileMap.GetTile(playerCell);

                if (tile != null && IsWaterTile(tile))
                {
                    isOnWater = true;
                    movementSpeed *= lavaSlowFactor;
                    return;
                }
            }
        }

        private bool IsWaterTile(TileBase tile)
        {
            if (tile == null || waterTiles == null) return false;

            foreach (TileBase waterTile in waterTiles)
            {
                if (tile == waterTile) return true;
            }

            return false;
        }

        //Applies lava Damage
        private void CheckLavaStatus()
        {
            Vector3Int playerCell = biomeTileMap.WorldToCell(transform.position);
            isOnLava = false;

            if (biomeTileMap != null)
            {
                TileBase tile = biomeTileMap.GetTile(playerCell);

                if (tile != null && IsLavaTile(tile))
                {
                    isOnLava = true;
                    return;
                }
            }
        }

        private bool IsLavaTile(TileBase tile)
        {
            if (tile == null || lavaTiles == null)
            {
                return false;
            }

            foreach (TileBase lavaTile in lavaTiles)
            {
                if (tile == lavaTile) return true;
            }

            return false;
        }

        private void ApplyLavaDamage()
        {
            if (!isOnLava)
            {
                return;
            }

            lavaDmgCooldown -= Time.deltaTime;

            if (lavaDmgCooldown <= 0f)
            {
                movementSpeed *= lavaSlowFactor;
                TakeDamage(lavaDmgPerSecond);
                lavaDmgCooldown = 1f;
                Debug.Log($"Player too DMG! Health: {playerHealth}");

                if (playerHealth <= 0)
                {
                    Die();
                }
            }
        }

        public void TakeDamage(float damage)
        {
            playerHealth -= damage;
            Debug.Log($"Player too {damage} DMG! Health: {playerHealth}");

            if (playerHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Player Died");

            SceneManager.LoadScene("DeathScene");
            Destroy(this.gameObject);

        }

        private void UpdateBiomeStatus()
        {
            if (dungeonGenerator == null)
            {
                Debug.LogWarning("DungeonGenerator is null in playercontroller");
                return;
            }

            List<DungeonGenerator.Room> rooms = dungeonGenerator.GetGeneratedRooms();
            BiomeType detectedBiome = BiomeType.Normal;

            Vector3 playerPos = transform.position;

            for (int i = 0; i < rooms.Count; i++)
            {
                DungeonGenerator.Room room = rooms[i];

                if (playerPos.x > room.x && playerPos.x < room.x + room.width && playerPos.y > room.y &&
                    playerPos.y < room.y + room.height)
                {
                    detectedBiome = dungeonGenerator.GetBiome(i);
                    break;
                }
            }

            currentBiome = detectedBiome;
        }

        public BiomeType GetCurrentBiome()
        {
            return currentBiome;
        }

        //Healing Function
        public void Heal(float healAmount)
        {
            playerHealth += Mathf.Min(playerHealth + healAmount, 100);
        }
        
        //Trap Functions

        private void CheckTrapStatus()
        {
            Vector3Int playerCell = trapTileMap.WorldToCell(transform.position);
            TileBase tile = trapTileMap.GetTile(playerCell);

            if (tile != null && IsTrapTile(tile))
            {
                if (Time.time - lastTrapTriggerTime > dungeonSettings.TrapCooldown)
                {
                    TriggerTrap();
                    lastTrapTriggerTime = Time.time;
                }
            }
        }

        private bool IsTrapTile(TileBase tile)
        {
            if (tile == null || trapTiles == null) return false;

            foreach (TileBase trapTile in trapTiles)
            {
                if (tile == trapTile) return true;
            }
            
            return false;
        }

        private void TriggerTrap()
        {
            playerHealth -= dungeonSettings.TrapDamage;

            if (playerHealth <= 0)
            {
                Die();
            }
        }

        public void SetDungeonGenerator(DungeonGenerator generator)
        {
            dungeonGenerator = generator;
        }

        public void SetGameManager(GameManager manager)
        {
            gameManager = manager;
        }
        
        public void SetTilemaps(Tilemap Floor, Tilemap Wall, Tilemap Decoration, Tilemap portal, Tilemap biome)
        {
            floorTilemap = Floor;
            wallTilemap = Wall;
            decorationTilemap = Decoration;
            portalTilemap = portal;
            biomeTileMap = biome;

            if (portal != null)
            {
                portalTilemap = portal;
            }
        }

        public void SetTrapTiles(TileBase[] traps)
        {
            trapTiles = traps;
        }
        
        public bool GetIsOnWater() => isOnWater;
        public bool GetIsOnLava() => isOnLava;
        public float GetHealth() => playerHealth;
        public Transform GetTransform() => transform;
        public bool IsAlive() => playerHealth > 0;
    }
}