using System.Collections;
using System.Collections.Generic;
using ProceduralDungeon.Combat;
using ProceduralDungeon.Enemy;
using UnityEngine;
using ProceduralDungeon.Generator;
using ProceduralDungeon.Items;
using ProceduralDungeon.Managers;
using ProceduralDungeon.Map;
using ProceduralDungeon.Settings;
using UnityEngine.Tilemaps;
using ProceduralDungeon.Player;
using UnityEngine.Rendering.VirtualTexturing;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject playerPrefab;

    [SerializeField] Tilemap portalTileMap;
    [SerializeField] Tilemap floorTilemap;
    [SerializeField] Tilemap wallTilemap;
    [SerializeField] Tilemap decorationTilemap;
    [SerializeField] Tilemap trapTilemap;
    [SerializeField] Tilemap biomeTileMap;

    [SerializeField] private DungeonGenerator dungeonGenerator;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private DungeonSettings dungeonSettings;

    [SerializeField] private ProceduralDungeon.Managers.DifficultyManager difficultyManager;
    [SerializeField] private ItemManager itemManager;


    void Start()
    {
        
        Random.InitState(dungeonSettings.Seed);
        
        if (difficultyManager == null)
        {
            difficultyManager = GetDifficultyManager();
        }

        if (difficultyManager == null)
        {
            GameObject difficultyManagerObj = new GameObject("DifficultyManager");
            difficultyManager = difficultyManagerObj.AddComponent<ProceduralDungeon.Managers.DifficultyManager>();
        }
        
        if (itemManager == null)
        {
            itemManager = GetItemManager();
        }

        if (itemManager == null)
        {
            GameObject itemManagerObj = new GameObject("ItemManager");
            itemManager = itemManagerObj.AddComponent<ItemManager>();
        }

        if (dungeonGenerator != null)
        {
            dungeonGenerator.GenerateDungeon(dungeonSettings.Seed);

            SpawnEnemiesInRooms();
        }
        else
        {
            Debug.LogError("[GM] Generator is not refrenced");
        }

        StartCoroutine(SpawnWhenWorldReady());
    }

    private ItemManager GetItemManager()
    {
        var managers = UnityEngine.Object.FindObjectsByType<ItemManager>(FindObjectsSortMode.None);
        return managers.Length > 0 ? managers[0] : null;
    }
    
    private DifficultyManager GetDifficultyManager()
    {
        var managers = UnityEngine.Object.FindObjectsByType<DifficultyManager>(FindObjectsSortMode.None);
        return managers.Length > 0 ? managers[0] : null;
    }
    
    private void SpawnEnemiesInRooms()
    {
        if (enemySpawner == null)
        {
            return;
        }

        if (dungeonGenerator == null)
        {
            return;
        }

        List<DungeonGenerator.Room> rooms = dungeonGenerator.GetGeneratedRooms();

        if (rooms == null || rooms.Count == 0)
        {
            return;
        }

        for (int i = 1; i < rooms.Count - 1; i++)
        {
            DungeonGenerator.Room currentRoom = rooms[i];
            Rect roomBounds = new Rect(currentRoom.x, currentRoom.y, currentRoom.width, currentRoom.height);

            if (currentRoom.isSpawnRoom || currentRoom.isEndRoom)
            {
                continue;
            }

            enemySpawner.SpawnEnemiesInRoom(roomBounds, i);
        }
    }

    private IEnumerator SpawnWhenWorldReady()
    {
        yield return new WaitForEndOfFrame();

        GameObject playerInstance = SpawnPlayer();

        if (playerInstance != null)
        {
            SetupCamera(playerInstance.transform);
        }
    }

    private GameObject SpawnPlayer()
    {
        if (playerPrefab == null || portalTileMap == null)
        {
            Debug.LogError("No player prefab or portalTileMap assigned!");
            return null;
        }

        if (dungeonSettings == null || dungeonSettings.SpawnTile == null)
        {
            Debug.LogError("No Dungeon settings or spawn tile assigned!");
        }

        BoundsInt bounds = portalTileMap.cellBounds;
        Vector3 spawnWorldPos = Vector3.zero;
        bool foundSpawn = false;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = portalTileMap.GetTile(pos);

            if (tile == dungeonSettings.SpawnTile)
            {
                spawnWorldPos += portalTileMap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0f);
                foundSpawn = true;
                break;
            }
        }

        if (!foundSpawn)
        {
            Debug.LogError("No Spawn tile found!");
            return null;
        }

        GameObject playerInstance = Instantiate(playerPrefab, spawnWorldPos, Quaternion.identity);

        PlayerController playerController = playerInstance.GetComponent<PlayerController>();

        if (playerController != null)
        {
            playerController.SetTilemaps(floorTilemap, wallTilemap, decorationTilemap, portalTileMap, biomeTileMap);
            playerController.SetDungeonGenerator(dungeonGenerator);
            playerController.SetTrapTiles(dungeonSettings.TrapTiles);
            playerController.SetGameManager(this);
        }
        else
        {
            Debug.LogError("Player controller cannot be found or instantiated!");
        }

        SecretRoomTrigger secretRoomTrigger =
            wallTilemap.gameObject.AddComponent<ProceduralDungeon.Map.SecretRoomTrigger>();
        secretRoomTrigger.SetSecretRoomGenerator(dungeonGenerator.GetSecretRooms());
        
        Debug.Log($"Player Spawned at {spawnWorldPos}");
        return playerInstance;
    }

    private void SetupPlayerCombat(Transform playerTransform)
    {
        PlayerCombat combat = playerTransform.GetComponent<PlayerCombat>();

        if (combat == null)
        {
            combat = playerTransform.gameObject.AddComponent<PlayerCombat>();
        }

        Debug.Log("[GM] Player combat Initialized");
    }

    private void SetupCamera(Transform playerTransform)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        CameraController cameraController = mainCamera.GetComponent<CameraController>();

        if (cameraController != null)
        {
            cameraController = mainCamera.gameObject.AddComponent<CameraController>();
        }

        cameraController.SetPlayerTransform(playerTransform);
    }

    public void ApplyDifficultyScaling(float difficultyMultiplier)
    {
        BaseEnemy[] allEnemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.InstanceID);

        foreach (BaseEnemy enemy in allEnemies)
        {
            Debug.Log($"[SPAWNER] Applied Difficulty {difficultyMultiplier}x");
        }
    }

    public void GenerateNewDungeon(int? customSeed = null)
    {
        BaseEnemy[] allEnemies = UnityEngine.Object.FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
        foreach (BaseEnemy enemy in allEnemies)
        {
            Destroy(enemy.gameObject);
        }
        
        Key[] allKeys = UnityEngine.Object.FindObjectsByType<Key>(FindObjectsSortMode.None);
        foreach (Key key in allKeys)
        {
            Destroy(key.gameObject);
        }
        
        Chest[] allChests = UnityEngine.Object.FindObjectsByType<Chest>(FindObjectsSortMode.None);
        foreach (Chest chest in allChests)
        {
            Destroy(chest.gameObject);
        }
        
        Debug.Log("Generating new dungeon");

        if (difficultyManager != null)
        {
            difficultyManager.IncrementDungeonLevel();
        }
        
        int currentSeed = dungeonSettings.Seed;
        int newSeed;

        do
        {
            newSeed = Random.Range(0, 1000000);
        } while (newSeed == currentSeed);

        dungeonSettings.SetSeed(newSeed);
        Random.InitState(newSeed);
        
        if (enemySpawner != null)
        {
            dungeonGenerator.ResetDungeon();
        }

        if (dungeonGenerator != null)
        {
            dungeonGenerator.ResetDungeon();
        }

        if (dungeonGenerator != null)
        {
            dungeonGenerator.GenerateDungeon(newSeed);
            SpawnEnemiesInRooms();
        }

        TeleportPlayerToSpawn();
    }

    private void TeleportPlayerToSpawn()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null || portalTileMap == null || dungeonGenerator == null)
        {
            return;
        }

        BoundsInt bounds = portalTileMap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = portalTileMap.GetTile(pos);

            if (tile == dungeonSettings.SpawnTile)
            {
                Vector3 spawnPos = portalTileMap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0f);
                player.transform.position = spawnPos;

                return;
            }
        }

        Debug.LogError("No Spawn tile found!");
    }
}