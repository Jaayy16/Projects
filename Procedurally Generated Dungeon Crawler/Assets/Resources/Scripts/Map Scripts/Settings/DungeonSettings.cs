using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace ProceduralDungeon.Settings
{
    [System.Serializable]
    public class DungeonSettings
    {
        public enum RoomShapes
        {
            Square,
            Hexagon,
            Circle
        }
        
        /// <summary>
        /// Dungeon Generation 
        /// </summary>
        [FoldoutGroup("Dungeon Dimensions")] [SerializeField]
        private int dungeonWidth = 80;
        
        [FoldoutGroup("Dungeon Dimensions")] [SerializeField]
        private int dungeonHeight = 60;
        
        /// <summary>
        /// Room Generation 
        /// </summary>
        
        [FoldoutGroup("Room Generation")] [SerializeField]
        private int minRoomWidth = 5;

        [FoldoutGroup("Room Generation")] [SerializeField]
        private int maxRoomWidth = 15;

        [FoldoutGroup("Room Generation")] [SerializeField]
        private int minRoomHeight = 5;

        [FoldoutGroup("Room Generation")] [SerializeField]
        private int maxRoomHeight = 20;

        [FoldoutGroup("Room Generation")] [SerializeField]
        private int maxRooms = 20;

        [FoldoutGroup("Room Generation")] [SerializeField]
        private int roomSpacing = 10;
        
        /// <summary>
        /// Corridor Generation 
        /// </summary>
        
        [FoldoutGroup("Corridor Generation")] [SerializeField]
        private int corridorWidth = 5;

        /// <summary>
        /// Vegetation Settings 
        /// </summary>
        
        [FoldoutGroup("Vegetation Settings")] [SerializeField]
        private bool enableVegetation = true;
        
        [FoldoutGroup("Vegetation Settings")] [SerializeField]
        private TileBase[] mossTiles = new TileBase[3];
        
        [FoldoutGroup("Vegetation Settings")] [SerializeField]
        private TileBase[] vineTiles = new TileBase[3];
                
        ///<summary>
        /// Chest Settings
        /// </summary>

        [FoldoutGroup("Chest Settings")] [SerializeField]
        private GameObject chestPrefab;
        
        /// <summary>
        /// Trap Settings
        /// </summary>

        [FoldoutGroup("Trap Settings")] [SerializeField]
        private bool enableTraps = true;
        
        [FoldoutGroup("Trap Settings")] [SerializeField]
        private float trapChance = 0.1f;

        [FoldoutGroup("Trap Settings")] [SerializeField]
        private TileBase[] trapTiles = new TileBase[1];

        [FoldoutGroup("Trap Settings")] [SerializeField]
        private float trapDamage = 15f;

        [FoldoutGroup("Trap Settings")] [SerializeField]
        private float trapCooldown = 2f;

        /// <summary>
        /// Decoration Settings
        /// </summary>
        
        [FoldoutGroup("Decoration Generation")] [SerializeField]
        private bool enableDecoration = true;
        [FoldoutGroup("Decoration Generation")] [SerializeField]
        public float spawnChance = 0.15f;
        [FoldoutGroup("Decoration Generation")] [SerializeField]
        private int minDistFromCentre = 2;
        [FoldoutGroup("Decoration Generation")] [SerializeField]
        private float decorationChance = 0.15f;
        
        /// <summary>
        /// Seed Generation 
        /// </summary>
        
        [FoldoutGroup("Seed Generation")] [SerializeField]
        private int seed = 12345;

        ///<summary>
        ///Tile Variance
        /// </summary>
        
        [FoldoutGroup("Tile Variance")] [SerializeField]
        private bool enableFloorVariance = true;
        
        [FoldoutGroup("Tile Variance")] [SerializeField]
        private bool enableWallVariance = true;

        [FoldoutGroup("Tile Variance")] [SerializeField]
        private float noiseScale = 1f;
        
        [FoldoutGroup("Tile Variance")] [SerializeField]
        private TileBase[] floorVariants = new TileBase[1];
        
        [FoldoutGroup("Tile Variance")] [SerializeField]
        private TileBase[] wallVariants = new TileBase[1];
        
        [FoldoutGroup("Tile Variance")] [SerializeField, Range(0, 1)]
        private float varianceThreshold = 0.5f;
        
        ///<summary>
        ///Contextual Tile Variance
        /// </summary>

        [FoldoutGroup("Corner & Props Settings")] [SerializeField]
        private bool enableContextualTiling = true;

        [FoldoutGroup("Corner & Props Settings")] [SerializeField]
        private TileBase cornerTile;
        
        [FoldoutGroup("Corner & Props Settings")] [SerializeField]
        private bool enableContextualProps = true;
        
        [FoldoutGroup("Corner & Props Settings")] [SerializeField]
        private TileBase[] floodedProps = new TileBase[1];
        
        [FoldoutGroup("Corner & Props Settings")] [SerializeField]
        private TileBase[] moltenProps = new TileBase[1];

        [FoldoutGroup("Corner & Props Settings")] [SerializeField]
        private float contextualPropChance = 0.2f;

        ///<summary>
        ///Biome Settings
        /// </summary>
        
        [FoldoutGroup("Biome Settings")] [SerializeField]
        private bool enableBiomes = true;

        [FoldoutGroup("Biome Settings")] [SerializeField]
        private float floodChance = 0.4f;
        
        [FoldoutGroup("Biome Settings")] [SerializeField]
        private float moltenChance = 0.3f;
        
        [FoldoutGroup("Biome Settings")] [SerializeField]
        private TileBase[] floodedTiles = new TileBase[1];
         
        [FoldoutGroup("Biome Settings")] [SerializeField]
        private TileBase[] moltenTiles = new TileBase[1];
        
        /// <summary>
        /// Secret Room Settings 
        /// </summary>

        [FoldoutGroup("Secret Room Settings")] [SerializeField] 
        private bool enableSecretRoom = true;

        [FoldoutGroup("Secret Room Settings")] [SerializeField]
        private int maxSecretRooms = 2;

        [FoldoutGroup("Secret Room Settings")] [SerializeField]
        private TileBase secretRoomEntranceTile;

        [FoldoutGroup("Secret Room Settings")] [SerializeField]
        private float secretRoomChance = 0.3f;
        
        /// <summary>
        /// Tile Refrences 
        /// </summary>

        [FoldoutGroup("Tile References")] [SerializeField]
        private TileBase wallTile;
        
        [FoldoutGroup("Tile References")] [SerializeField]
        private TileBase floorTile; 
        
        [FoldoutGroup("Tile References")] [SerializeField]
        private TileBase[] trapTile = new TileBase[1];
        
        [FoldoutGroup("Tile References")] [SerializeField]
        private TileBase[] decorationTiles = new TileBase[1];
        
        [FoldoutGroup("Tile References")] [SerializeField]
        private TileBase spawnTile;
        
        [FoldoutGroup("Tile References")] [SerializeField]
        private TileBase levelExitTile;
        
        public void SetSeed(int newSeed)
        {
            seed = newSeed;
        }
        
        //Pass Secret Room-Related Variables
        public bool EnableSecretRoom => enableSecretRoom;
        public int MaxSecretRooms => maxSecretRooms;
        public TileBase SecretRoomEntranceTile => secretRoomEntranceTile;
        public float SecretRoomChance => secretRoomChance;

        //Pass Dungeon-Related Variables
        public int DungeonWidth  => dungeonWidth;
        public int DungeonHeight => dungeonHeight;
        
        //Pass Room-Related Variables
        public int MinRoomWidth => minRoomWidth;
        public int MaxRoomWidth => maxRoomWidth;
        public int MinRoomHeight => minRoomHeight;
        public int MaxRoomHeight => maxRoomHeight;
        public int MaxRooms => maxRooms;
        public int RoomSpacing => roomSpacing;
        
        //Pass Corridor-Related Variables
        public int CorridorWidth => corridorWidth;
        
        //Pass Vegetation-Related Variables
        public bool EnableVegetation => enableVegetation;
        public TileBase[] MossTiles => mossTiles;
        public TileBase[] VineTiles => vineTiles;

        //Pass Contextual Tile Variables
        public bool EnableContextualTiling => enableContextualTiling;
        public TileBase CornerTile => cornerTile;
        public bool EnableContextualProps => enableContextualProps;
        public TileBase[] FloodedProps => floodedProps;
        public TileBase[] MoltenProps => moltenProps;
        public float ContextualPropChance => contextualPropChance;
        
        //Pass Chest-Related Variables
        public GameObject ChestPrefab => chestPrefab;
        
        //Pass Trap-Related Variables
        
        public bool EnableTraps => enableTraps;
        public float TrapChance => trapChance;
        public TileBase[] TrapTiles => trapTiles;
        public float TrapDamage => trapDamage;
        public float TrapCooldown => trapCooldown;
        
        //Pass Decoration-Related Variables
        public bool EnableDecoration => enableDecoration;
        public int MinDistFromCentre => minDistFromCentre;
        public float DecorationChance => decorationChance;

        public int Seed
        {
            get => seed;
            set => seed = value;
        }
        
        //Pass Biome Related Variables
        public bool EnableBiomes => enableBiomes;
        public float FloodChance => floodChance;
        public float MoltenChance => moltenChance;
        public TileBase[] FloodedTiles => floodedTiles;
        public TileBase[] MoltenTiles => moltenTiles;
        
        //Pass Perlin-Noise Related Variables
        public bool EnableFloorVariance => enableFloorVariance;
        public bool EnableWallVariance => enableWallVariance;
        public float NoiseScale => noiseScale;
        public TileBase[] FloorVariants => floorVariants;
        public TileBase[] WallVariants => wallVariants;
        public float VarianceThreshold => varianceThreshold;
        
        //Pass Tile-Related Variables
        public TileBase WallTile => wallTile;
        public TileBase FloorTile => floorTile;
        public TileBase[] TrapTile => trapTile;
        public TileBase[] DecorationTiles => decorationTiles;
        public TileBase SpawnTile => spawnTile;
        public TileBase LevelExitTile => levelExitTile;
        
    }
}
