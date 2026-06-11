using System.Collections.Generic;
using ProceduralDungeon.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProceduralDungeon.Generator
{
    public class SecretRoomGenerator : MonoBehaviour
    {
        
        private DungeonSettings dungeonSettings;
        private int Seed;
        private List<SecretRoomData> secretRooms = new List<SecretRoomData>();

        public SecretRoomGenerator(DungeonSettings settings, int randSeed)
        {
            dungeonSettings = settings;
            Seed = randSeed;
        }
        
        public void GenerateSecretRoomsEntrance(List<DungeonGenerator.Room> rooms, Tilemap wallTileMap, Tilemap decorationTilemap, bool[,] wallData)
        {
            if (!dungeonSettings.EnableSecretRoom || dungeonSettings.SecretRoomEntranceTile == null)
            {
                return;
            }
            
            System.Random SecretRNG = new System.Random(Seed + 7000);
            int secretRoomsPlaced = 0;
            secretRooms.Clear();
            
            List<Vector3Int> validEntranceSpots = new List<Vector3Int>();

            foreach (DungeonGenerator.Room room in rooms)
            {
                if (room.isSpawnRoom || room.isEndRoom) continue;

                for (int x = room.x - 1; x <= room.x + room.width; x++)
                {
                    for (int y = room.y - 1; y <= room.y + room.height; y++)
                    {
                        if (x < 0 || x >= dungeonSettings.DungeonWidth || y < 0 || y >= dungeonSettings.DungeonHeight)
                            continue;
                        
                        Vector3Int pos = new Vector3Int(x, y, 0);

                        if (wallTileMap.GetTile(pos) != null)
                        {
                            bool isRoomWall = IsRoomWall(x, y, room, rooms);

                            if (isRoomWall)
                            {
                                validEntranceSpots.Add(pos);
                            }
                        }
                    }
                }
            }

            while (secretRoomsPlaced < dungeonSettings.MaxSecretRooms && validEntranceSpots.Count > 0)
            {
                if (SecretRNG.NextDouble() < dungeonSettings.SecretRoomChance)
                {
                    int randomIndex = SecretRNG.Next(validEntranceSpots.Count);
                    Vector3Int entrancePos = validEntranceSpots[randomIndex];
                    validEntranceSpots.RemoveAt(randomIndex);
                    
                    wallTileMap.SetTile(entrancePos, null);
                    decorationTilemap.SetTile(entrancePos, dungeonSettings.SecretRoomEntranceTile);
                    
                    SecretRoomData secretRoom = new SecretRoomData(entrancePos, dungeonSettings.SecretRoomEntranceTile);
                    
                    secretRooms.Add(secretRoom);
                    
                    secretRoomsPlaced++;
                }
                else
                {
                    if (validEntranceSpots.Count > 0)
                    {
                        validEntranceSpots.RemoveAt(SecretRNG.Next(validEntranceSpots.Count));
                    }
                }
            }
        }

        private bool IsRoomWall(int x, int y, DungeonGenerator.Room targetRoom, List<DungeonGenerator.Room> allRooms)
        {
            bool adjToRoom = false;

            for (int xOff = -1; xOff <= 1; xOff++)
            {
                for (int yOff = -1; yOff <= 1; yOff++)
                {
                    if (xOff == 0 && yOff == 0) continue;
                    
                    int checkX = x + xOff;
                    int checkY = y + yOff;

                    if (checkX >= targetRoom.x && checkX < targetRoom.x + targetRoom.width && checkY >= targetRoom.y &&
                        checkY < targetRoom.y + targetRoom.height)
                    {
                        bool isOtherRoom = false;

                        foreach (DungeonGenerator.Room room in allRooms)
                        {
                            if (room.x == targetRoom.x && room.y == targetRoom.y && room.width == targetRoom.width 
                                && room.height == targetRoom.height) 
                                continue;

                            if (room.IsPointInRoom(x, y))
                            {
                                isOtherRoom = true;
                                break;
                            }
                            
                        }

                        if (!isOtherRoom)
                        {
                            adjToRoom = true;
                            break;
                        }
                    }
                }

                if (adjToRoom) break;
            }
            return adjToRoom;
        }

        public void DiscoverSecretRoom(Vector3Int pos)
        {
            foreach (SecretRoomData secretRoom in secretRooms)
            {
                if (secretRoom.entrancePos == pos)
                {
                    secretRoom.isDiscovered = true;
                    break;
                }
            }
        }

        public bool IsSecretRoomEntrance(Vector3Int pos)
        {
            foreach (SecretRoomData secretRoom in secretRooms)
            {
                if (secretRoom.entrancePos == pos)
                {
                    return true;
                }
            }
            return false;
        }
        
        public List<SecretRoomData> GetSecretRooms() => secretRooms;
        
    }
}