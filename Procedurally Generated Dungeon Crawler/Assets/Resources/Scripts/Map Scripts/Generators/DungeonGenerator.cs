using System;
using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using ProceduralDungeon.Combat;
using ProceduralDungeon.Enemy;
using ProceduralDungeon.Items;
using ProceduralDungeon.Settings;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace ProceduralDungeon.Generator
{
    [ExecuteAlways]
    public class DungeonGenerator : MonoBehaviour
    {   
        [Header("Dungeon Settings")]
        [FoldoutGroup("Settings")]
        [SerializeField] private DungeonSettings dungeonSettings;

        //Secret Room
        private SecretRoomGenerator secretRoomGenerator;
        
        //Vegetation
        private VegitationGenerator vegetationGenerator;
        private VegetationRenderer vegetationRenderer;
        private bool[,] lastTileData;
        
        //Perlin-noise generated call
        private TileVarianceGenerator varianceGenerator;
        
        //Biome Generation Call
        private BiomeGenerator biomeGenerator;
        private List<BiomeType> roomBiomesCache = new  List<BiomeType>();
        
        //Tilemaps and Settings
        [Header("Dungeon Generation")]
        
        [FoldoutGroup("Generation")]
        [SerializeField] private Tilemap floorTileMap;
        
        [FoldoutGroup("Generation")]
        [SerializeField] private Tilemap wallTileMap;
        
        [FoldoutGroup("Generation")]
        [SerializeField] private Tilemap trapTileMap;
        
        [FoldoutGroup("Generation")]
        [SerializeField] private Tilemap decorationTileMap;
        
        [FoldoutGroup("Generation")]
        [SerializeField] private Tilemap portalTileMap;
        
        [FoldoutGroup("Generation")] 
        [SerializeField] private Tilemap biomeTileMap;
        
        ///<Summary>
        /// Dungeon Generation and Painting Below
        /// <Summary>
        
        // struct for rooms and all necessary aspects of it
        [System.Serializable]
        public struct Room
        {
            public int x, y;
            public int width, height;
            public DungeonSettings.RoomShapes shape;
            public bool isSpawnRoom;
            public bool isEndRoom;

            public Room(int x, int y, int width, int height, DungeonSettings.RoomShapes shape, bool isSpawnRoom,
                bool isEndRoom)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
                this.shape = shape;
                this.isSpawnRoom = isSpawnRoom;
                this.isEndRoom = isEndRoom;
            }

            public Vector2Int GetCentre()
            {
                return new Vector2Int(x + width / 2, y + height / 2);
            }

            public bool IsInRoom(int xPer, int yPer)
            {
                return xPer > x && xPer < x + width - 1 && yPer > y && yPer < y + height - 1;
            }

            public bool IsPointInRoom(int xPer, int yPer)
            {
                switch (shape)
                {
                    case DungeonSettings.RoomShapes.Square:
                        return xPer >= x && xPer < x + width && yPer >= y && yPer < y + height;
                    case DungeonSettings.RoomShapes.Hexagon:
                        return IsPointInHex(xPer, yPer);
                    case DungeonSettings.RoomShapes.Circle:
                        return IsPointInCircle(xPer, yPer);
                    default:
                        return false;
                }
            }

            private bool IsPointInHex(int xPer, int yPer)
            {
                int xCentre = x + width / 2;
                int yCentre = y + height / 2;
                int xRad = width / 2;
                int yRad = height / 2;

                int xDeg = Mathf.Abs(xPer - xCentre);
                int yDeg = Mathf.Abs(yPer - yCentre);

                if (xDeg > xRad || yDeg > yRad) return false;
                return yDeg < yRad * (1f - (float)xDeg / xRad);
            }

            private bool IsPointInCircle(int xPer, int yPer)
            {
                int xCentre = x + width / 2;
                int yCentre = y + height / 2;
                int xRad = width / 2;
                int yRad = height / 2;

                if (xRad == 0 || yRad == 0) return false;

                float xDeg = xPer - xCentre;
                float yDeg = yPer - yCentre;
                float norm = (xDeg * xDeg) / (xRad * xRad) + (yDeg * yDeg) / (yRad * yRad);
                return norm <= 1f;
            }
        }
        
        private List<Room> generatedRooms = new List<Room>();
        
        [Button("Generate Dungeon")]
        public void GenerateDungeon(int? customSeed = null)
        {
            int seedToUse = customSeed ?? dungeonSettings.Seed;
            
            floorTileMap.ClearAllTiles();
            wallTileMap.ClearAllTiles();
            decorationTileMap.ClearAllTiles();
            trapTileMap.ClearAllTiles();
            portalTileMap.ClearAllTiles();
            biomeTileMap.ClearAllTiles();

            varianceGenerator = new TileVarianceGenerator(dungeonSettings, seedToUse);
            biomeGenerator = new BiomeGenerator(dungeonSettings, seedToUse);
            
            generatedRooms = GenerateRooms(seedToUse);
            Debug.Log($"Generated {generatedRooms.Count} rooms");

            if (dungeonSettings.EnableBiomes)
            {
                biomeGenerator.AssignBiome(generatedRooms.Count);
                CacheRoomBiomes(generatedRooms);
            }
            
            GenerateCorridors(generatedRooms);

            bool[,] tileData = PaintDungeonTiles(generatedRooms);

            lastTileData = tileData;
            
            if (dungeonSettings.EnableBiomes)
            {
                ApplyRoomBiomes(generatedRooms);
                ApplyCorridorBiomes(generatedRooms);
            }
            
            GenerateDecorations(generatedRooms, tileData);

            if (dungeonSettings.EnableVegetation)
            {
                vegetationGenerator = new VegitationGenerator(dungeonSettings, seedToUse, biomeGenerator);
                vegetationGenerator.InitializeVeg(generatedRooms, floorTileMap, biomeTileMap, tileData);

                if (vegetationRenderer == null)
                {
                    vegetationRenderer = gameObject.AddComponent<VegetationRenderer>();
                    vegetationRenderer.SetVegetationGenerator(vegetationGenerator);
                }
            }
            
            CornerTileGenerator cornerTileGenerator = new CornerTileGenerator(dungeonSettings);
            cornerTileGenerator.GenerateCornerTiles(generatedRooms, wallTileMap);

            if (dungeonSettings.EnableBiomes)
            {
                ContextualPropGenerator propGenerator = new ContextualPropGenerator(dungeonSettings, seedToUse);
                propGenerator.GenerateContextualProps(generatedRooms, biomeTileMap, decorationTileMap, biomeGenerator, tileData);
            }
            
            TrapGenerator trapGenerator = new TrapGenerator(dungeonSettings, seedToUse);
            trapGenerator.GenerateTraps(generatedRooms, floorTileMap, trapTileMap, tileData);
            
            ChestGenerator chestGenerator = new ChestGenerator(dungeonSettings, seedToUse);
            chestGenerator.GenerateChestsInDungeon(generatedRooms, floorTileMap, dungeonSettings.ChestPrefab, tileData);

            secretRoomGenerator = new SecretRoomGenerator(dungeonSettings, seedToUse);
            secretRoomGenerator.GenerateSecretRoomsEntrance(generatedRooms, wallTileMap, floorTileMap, tileData);
            
        }

        [Button("Reset Dungeon")]
        public void ResetDungeon()
        {
            if (floorTileMap != null || wallTileMap != null || trapTileMap != null || decorationTileMap != null)
            {
                Debug.Log("Reset Dungeon");
                floorTileMap.ClearAllTiles();
                wallTileMap.ClearAllTiles();
                decorationTileMap.ClearAllTiles();
                trapTileMap.ClearAllTiles();
                portalTileMap.ClearAllTiles();
                biomeTileMap.ClearAllTiles();
                
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
                
            }
            else
            {
                Debug.Log("Dungeon Tilemap is null");
            }
        }

        public SecretRoomGenerator GetSecretRooms()
        {
            return secretRoomGenerator;
        }
        
        private void Update()
        {
            if (vegetationRenderer != null && dungeonSettings.EnableVegetation && lastTileData != null)
            {
                vegetationGenerator.UpdateVeg(Time.deltaTime, decorationTileMap, lastTileData);
            }
        }

        //Generates rooms in a linear position to each other 
        private List<Room> GenerateRooms(int seed)
        {
            List<Room> rooms = new List<Room>();
            System.Random rng = new System.Random(seed);

            int spawnW = rng.Next(dungeonSettings.MinRoomWidth, dungeonSettings.MaxRoomWidth + 1);
            int spawnH = rng.Next(dungeonSettings.MinRoomHeight, dungeonSettings.MaxRoomHeight + 1);

            DungeonSettings.RoomShapes spawnShape = DungeonSettings.RoomShapes.Square;

            Room spawnRoom = new Room(5, dungeonSettings.DungeonHeight / 2 - spawnH / 2, spawnW, spawnH, spawnShape,
                true, false);

            rooms.Add(spawnRoom);

            Debug.Log($"Spawning at {spawnW}x{spawnH}");

            for (int i = 1; i < dungeonSettings.MaxRooms - 1; i++)
            {
                int roomW = rng.Next(dungeonSettings.MinRoomWidth, dungeonSettings.MaxRoomWidth + 1);
                int roomH = rng.Next(dungeonSettings.MinRoomHeight, dungeonSettings.MaxRoomHeight + 1);

                int xPos = 5 + i * (dungeonSettings.MinRoomWidth + dungeonSettings.RoomSpacing);

                int yVar = rng.Next(-5, 6);
                int yPos = dungeonSettings.DungeonHeight / 2 - roomH / 2 + yVar;

                yPos = Mathf.Clamp(yPos, 1, dungeonSettings.DungeonHeight - roomH - 1);

                if (xPos + roomW >= dungeonSettings.DungeonWidth - 10)
                {
                    break;
                }

                DungeonSettings.RoomShapes roomShape = (DungeonSettings.RoomShapes)rng.Next(0, 3);

                Room newRoom = new Room(xPos, yPos, roomW, roomH, roomShape, false, false);
                rooms.Add(newRoom);
            }

            int endW = rng.Next(dungeonSettings.MinRoomWidth, dungeonSettings.MaxRoomWidth + 1);
            int endH = rng.Next(dungeonSettings.MinRoomHeight, dungeonSettings.MaxRoomHeight + 1);

            int endX = dungeonSettings.DungeonWidth - endW - 5;
            int endY = dungeonSettings.DungeonHeight - endH - 5;

            DungeonSettings.RoomShapes endShape = DungeonSettings.RoomShapes.Square;

            Room endRoom = new Room(endX, endY, endW, endH, endShape, false, true);
            rooms.Add(endRoom);

            Debug.Log($"End Room at {endW}x{endH}");

            return rooms;
        }

        //Generates all the corridors for all the rooms
        private void GenerateCorridors(List<Room> rooms)
        {
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                Vector2Int startRoom = rooms[i].GetCentre();
                Vector2Int endRoom = rooms[i + 1].GetCentre();

                CreateHorizontalCorridor(startRoom.x, endRoom.x, startRoom.y, rooms);
                CreateVerticalCorridor(startRoom.y, endRoom.y, endRoom.x, rooms);
            }
        }

        //Creates the horizontal corridors
        private void CreateHorizontalCorridor(int xStart, int xEnd, int yCentre, List<Room> rooms)
        {
            int xMin = Mathf.Min(xStart, xEnd);
            int xMax = Mathf.Max(xStart, xEnd);

            for (int x = xMin; x <= xMax; x++)
            {
                bool inRoom = false;
                foreach (Room room in rooms)
                {
                    if (room.IsPointInRoom(x, yCentre) && room.IsInRoom(x, yCentre))
                    {
                        inRoom = true;
                        break;
                    }
                }

                if (!inRoom)
                {
                    CarveCorridor(x, yCentre);
                }
            }
        }

        //Creates the vertical corridors
        private void CreateVerticalCorridor(int yStart, int yEnd, int xCentre, List<Room> rooms)
        {
            int yMin = Mathf.Min(yStart, yEnd);
            int yMax = Mathf.Max(yStart, yEnd);

            for (int y = yMin; y <= yMax; y++)
            {
                bool inRoom = false;
                foreach (Room room in rooms)
                {
                    if (room.IsPointInRoom(xCentre, y) && room.IsInRoom(xCentre, y))
                    {
                        inRoom = true;
                        break;
                    }
                }

                if (!inRoom)
                {
                    CarveCorridor(y, xCentre);
                }
            }
        }

        //carves out the corridors between the rooms on the grid
        private void CarveCorridor(int xCentre, int yCentre)
        {
            int corridorWidth = dungeonSettings.CorridorWidth;
            int corridorHalf = corridorWidth / 2;

            for (int offsetX = -corridorHalf; offsetX <= corridorHalf; offsetX++)
            {
                for (int offsetY = -corridorHalf; offsetY <= corridorHalf; offsetY++)
                {
                    int x = xCentre + offsetX;
                    int y = yCentre + offsetY;

                    if (x >= 0 && x < dungeonSettings.DungeonWidth && y >= 0 && y < dungeonSettings.DungeonHeight)
                    {
                        CreateTile(x, y);
                    }
                }
            }
        }

        //stores which rooms are assigned what biomes
        private void CacheRoomBiomes(List<Room> rooms)
        {
            roomBiomesCache = new List<BiomeType>();
            for (int i = 0; i < rooms.Count; i++)
            {
                BiomeType biome = biomeGenerator.GetBiome(i);
                roomBiomesCache.Add(biomeGenerator.GetBiome(i));
            }
        }

        //Generates the biomes for the corresponding room
        private void ApplyRoomBiomes(List<Room> rooms)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                Room room = rooms[i];
                BiomeType biome = biomeGenerator.GetBiome(i);
                bool isSquareRoom = room.shape == DungeonSettings.RoomShapes.Square;
                
                
                biomeGenerator.GenerateRoomBiome(biome, floorTileMap, biomeTileMap, room.x, room.y, room.width, room.height, isSquareRoom);
            }
        }

        private void ApplyCorridorBiomes(List<Room> rooms)
        {
            if (rooms == null || rooms.Count < 2)
            {
                return;
            }

            System.Random corridorRNG = new System.Random(dungeonSettings.Seed + 1000);

            for (int i = 0; i < rooms.Count; i++)
            {

                if (i >= roomBiomesCache.Count || i + 1 >= roomBiomesCache.Count)
                {
                    continue;
                }
                
                Room currentRoom = rooms[i];
                Room nextRoom = rooms[i + 1];
                
                Vector2Int startRoom = rooms[i].GetCentre();
                Vector2Int endRoom = rooms[i + 1].GetCentre();
                
                BiomeType biome1 = biomeGenerator.GetBiome(i);
                BiomeType biome2 = biomeGenerator.GetBiome(i + 1);
                
                int xMin = Mathf.Min(startRoom.x, endRoom.x);
                int xMax = Mathf.Max(startRoom.x, endRoom.x);
                int yMin = Mathf.Min(startRoom.y, endRoom.y);
                int yMax = Mathf.Max(startRoom.y, endRoom.y);

                biomeGenerator.BlendCorridorBiome(biome1, biome2, biomeTileMap, xMin, xMax, startRoom.y,
                    startRoom.y, corridorRNG);

                biomeGenerator.BlendCorridorBiome(biome1, biome2, biomeTileMap, endRoom.x, endRoom.x, yMin, yMax,
                    corridorRNG);
            }
        }

        public BiomeType GetBiome(int roomIndex)
        {
            if (biomeGenerator != null)
            {
                return biomeGenerator.GetBiome(roomIndex);
            }

            return BiomeType.Normal;
        }
        
        //Generates the decorations
        private void GenerateDecorations(List<Room> rooms, bool[,] tileData)
        {
            if (!dungeonSettings.EnableDecoration ||
                dungeonSettings.DecorationTiles == null ||
                dungeonSettings.DecorationTiles.Length == 0)
            {
                return;
            }

            System.Random rng = new System.Random(dungeonSettings.Seed + 1);
            int decoCount = 0;

            for (int x = 0; x < dungeonSettings.DungeonWidth; x++)
            {
                for (int y = 0; y < dungeonSettings.DungeonHeight; y++)
                {
                    if (!tileData[x, y]) continue;

                    if (!IsValidDecoSpot(x, y, rooms)) continue;

                    if (decorationTileMap.GetTile(new Vector3Int(x, y, 0)) != null) continue;
                    
                    if(biomeTileMap.GetTile(new Vector3Int(x, y, 0)) != null) continue;

                    if (rng.NextDouble() < dungeonSettings.DecorationChance)
                    {
                        TileBase decoTile =
                            dungeonSettings.DecorationTiles[rng.Next(0, dungeonSettings.DecorationTiles.Length)];

                        decorationTileMap.SetTile(new Vector3Int(x, y, 0), decoTile);
                        decoCount++;
                    }

                }
            }

            Debug.Log($"Place {decoCount} decorations");
        }

        //Checking to see if intended spawn position for decorations is valid
        private bool IsValidDecoSpot(int x, int y, List<Room> rooms)
        {
            int minDist = dungeonSettings.MinDistFromCentre;
            foreach (Room room in rooms)
            {
                if (room.isEndRoom || room.isSpawnRoom)
                {
                    Vector2Int roomCentre = room.GetCentre();
                    float dist = Vector2Int.Distance(new Vector2Int(x, y), roomCentre);

                    if (dist < minDist) return false;
                }
            }

            return true;
        }

        //Purposefully empty to be used later in the code
        private void CreateTile(int x, int y)
        {
        }

        //Paints Dungeon Tiles
        private bool[,] PaintDungeonTiles(List<Room> rooms)
        {

            TileVarianceGenerator varianceGenerator = new TileVarianceGenerator(dungeonSettings, dungeonSettings.Seed);
            
            //Marks Rooms
            bool[,] created = new bool[dungeonSettings.DungeonWidth, dungeonSettings.DungeonHeight];
            bool[,] isRoomTile = new bool[dungeonSettings.DungeonWidth, dungeonSettings.DungeonHeight];
            bool[,] isMainFloor = new bool[dungeonSettings.DungeonWidth, dungeonSettings.DungeonHeight];
            
            foreach (Room room in rooms)
            {
                for (int x = room.x - 1; x < room.x + room.width; x++)
                {
                    for (int y = room.y - 1; y < room.y + room.height; y++)
                    {
                        if (x >= 0 && x < dungeonSettings.DungeonWidth && y >= 0 && y < dungeonSettings.DungeonHeight)
                        {
                            if (room.IsPointInRoom(x, y))
                            {
                                created[x, y] = true;
                                isRoomTile[x, y] = true;

                                if (room.IsInRoom(x, y))
                                {
                                    isMainFloor[x, y] = true;
                                }
                                else
                                {
                                    isMainFloor[x, y] = false;
                                }
                            }
                        }
                    }
                }

                for (int x = room.x - 1; x < room.x + room.width + 1; x++)
                {
                    for (int y = room.y - 1; y < room.y + room.height + 1; y++)
                    {
                        if (x >= 0 && x < dungeonSettings.DungeonWidth &&
                            y >= 0 && y < dungeonSettings.DungeonHeight)
                        {

                            if (!isRoomTile[x, y] && !created[x, y])
                            {
                                bool adjToRoom = false;

                                for (int xDeg = -1; xDeg <= 1; xDeg++)
                                {
                                    for (int yDeg = -1; yDeg <= 1; yDeg++)
                                    {
                                        if (xDeg == 0 && yDeg == 0) continue;
                                        int xN = x + xDeg;
                                        int yN = y + yDeg;

                                        if (xN >= 0 && xN < dungeonSettings.DungeonWidth && yN >= 0 &&
                                            yN < dungeonSettings.DungeonHeight)
                                        {
                                            if (isRoomTile[xN, yN] && isMainFloor[xN, yN])
                                            {
                                                adjToRoom = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (adjToRoom) break;
                                }

                                if (adjToRoom)
                                {
                                    created[x, y] = true;
                                    isMainFloor[x, y] = false;
                                }
                            }
                        }
                    }
                }
            }

            //Marks corridors
            int corridorWidth = dungeonSettings.CorridorWidth;
            int corridorHalf = corridorWidth / 2;
            int floorWidth = corridorWidth - 2;
            int floorHalf = floorWidth / 2;

            bool[,] corridorCreated = new bool[dungeonSettings.DungeonWidth, dungeonSettings.DungeonHeight];
            bool[,] isCorridorFloor = new bool[dungeonSettings.DungeonWidth, dungeonSettings.DungeonHeight];

            for (int i = 0; i < rooms.Count - 1; i++)
            {
                Vector2Int startRoom = rooms[i].GetCentre();
                Vector2Int endRoom = rooms[i + 1].GetCentre();

                int xMin = Mathf.Min(startRoom.x, endRoom.x);
                int xMax = Mathf.Max(startRoom.x, endRoom.x);

                for (int x = xMin; x <= xMax; x++)
                {
                    for (int yOffset = -corridorHalf; yOffset <= corridorHalf; yOffset++)
                    {
                        int y = startRoom.y + yOffset;

                        if (y >= 0 && y < dungeonSettings.DungeonHeight)
                        {
                            bool inRoom = false;
                            foreach (Room room in rooms)
                            {
                                if (room.IsPointInRoom(x, y) && room.IsInRoom(x, y))
                                {
                                    inRoom = true;
                                    break;
                                }
                            }

                            if (!inRoom)
                            {
                                created[x, y] = true;
                                corridorCreated[x, y] = true;

                                if (yOffset >= -floorHalf && yOffset <= floorHalf)
                                {
                                    isMainFloor[x, y] = true;
                                    isCorridorFloor[x, y] = true;
                                }
                                else
                                {
                                    isMainFloor[x, y] = false;
                                }
                            }
                        }
                    }
                }

                int yMin = Mathf.Min(startRoom.y, endRoom.y);
                int yMax = Mathf.Max(startRoom.y, endRoom.y);

                for (int y = yMin; y <= yMax; y++)
                {
                    for (int xOffset = -corridorHalf; xOffset <= corridorHalf; xOffset++)
                    {
                        int x = endRoom.x + xOffset;

                        if (x >= 0 && x < dungeonSettings.DungeonWidth)
                        {
                            bool inRoom = false;
                            foreach (Room room in rooms)
                            {
                                if (room.IsPointInRoom(x, y) && room.IsInRoom(x, y))
                                {
                                    inRoom = true;
                                    break;
                                }
                            }

                            if (!inRoom)
                            {
                                created[x, y] = true;
                                corridorCreated[x, y] = true;

                                if (xOffset >= -floorHalf && xOffset <= floorHalf)
                                {
                                    isMainFloor[x, y] = true;
                                    isCorridorFloor[x, y] = true;
                                }
                                else
                                {
                                    isMainFloor[x, y] = false;
                                }
                            }
                        }
                    }
                }
            }

            // Fixes issue with some room corridors not having walls on all sides
            for (int x = 0; x < dungeonSettings.DungeonWidth; x++)
            {
                for (int y = 0; y < dungeonSettings.DungeonHeight; y++)
                {
                    if (corridorCreated[x, y] && isMainFloor[x, y])
                    {
                        for (int xDeg = -1; xDeg <= 1; xDeg++)
                        {
                            for (int yDeg = -1; yDeg <= 1; yDeg++)
                            {
                                if (xDeg == 0 && yDeg == 0) continue;

                                int xNeighbour = x + xDeg;
                                int yNeighbour = y + yDeg;

                                if (xNeighbour >= 0 && xNeighbour < dungeonSettings.DungeonWidth &&
                                    yNeighbour >= 0 && yNeighbour < dungeonSettings.DungeonHeight)
                                {
                                    if (!created[xNeighbour, yNeighbour] && !isRoomTile[xNeighbour, yNeighbour])
                                    {
                                        created[xNeighbour, yNeighbour] = true;
                                        isMainFloor[xNeighbour, yNeighbour] = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Paint All Wall Tiles
            for (int x = 0; x < dungeonSettings.DungeonWidth; x++)
            {
                for (int y = 0; y < dungeonSettings.DungeonHeight; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    
                    if (created[x, y] && !isMainFloor[x, y] && !isCorridorFloor[x, y])
                    {
                        TileBase wallTile = varianceGenerator.GetWallVariant(x, y, dungeonSettings.WallTile);
                        wallTileMap.SetTile(pos, wallTile);
                    }
                }
            }

            //Paint All Floor Tiles
            for (int x = 0; x < dungeonSettings.DungeonWidth; x++)
            {
                for (int y = 0; y < dungeonSettings.DungeonHeight; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);

                    if (isMainFloor[x, y] || isCorridorFloor[x, y])
                    {
                       TileBase floorTile = varianceGenerator.GetFloorVariant(x, y, dungeonSettings.FloorTile);
                       floorTileMap.SetTile(pos, floorTile); 
                    }
                }
            }
            
            //Places Spawn and Level Exit Tiles
            foreach (Room room in rooms)
            {
                Vector3Int roomCentre = new Vector3Int(room.GetCentre().x, room.GetCentre().y, 0);

                if (room.isSpawnRoom)
                {
                    portalTileMap.SetTile(roomCentre, dungeonSettings.SpawnTile);
                }
                else if (room.isEndRoom)
                {
                    portalTileMap.SetTile(roomCentre, dungeonSettings.LevelExitTile);
                }
            }

            //takes all floor tiles (corridor and room) and combines them into a boolean array to pass to generateDecorations
            bool[,] combined = new bool[dungeonSettings.DungeonWidth, dungeonSettings.DungeonHeight];

            for (int x = 0; x < dungeonSettings.DungeonWidth; x++)
            {
                for (int y = 0; y < dungeonSettings.DungeonHeight; y++)
                {
                    combined[x, y] = (isMainFloor[x, y]  && isRoomTile[x,y]) || isCorridorFloor[x, y];
                }
            }

            return combined;
        }

        public List<Room> GetGeneratedRooms()
        {
            return generatedRooms;
        }

    }
}