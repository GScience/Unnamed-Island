using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Entitys;
using Island.Game.Data.Blocks;
using Island.Game.Render;
using Island.Game.World;
using Island.UI;
using Island.UI.Pannels;
using Island.Utils;
using UnityEngine;

namespace Island.Game.System
{
    /// <summary>
    /// 世界管理器
    /// 负责加载世界、自动加载Chunk等
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        private const string DebugWorldName = "_DEBUG-WORLD_" + "191215.1";

        /// <summary>
        /// 标记是否在初始化（第一次初始化所有Chunk状态）
        /// </summary>
        public bool IsInitializing { get; private set; } = true;

        public Vector3 BlockSize => worldInfo.blockSize;
        public Vector3Int ChunkSize => worldInfo.chunkSize;

        public int chunkPool = 100;
        public Anchor worldAnchor;

        public int sight = 2;

        public Transform chunkParent;

        public int physicalReadyChunkCount = 0;
        public bool isLoadingChunk = false;

        // 寻找到的Chunk表
        /*
         *  -----------
         * 4| | | | | |
         *  -----------
         *  -----------
         * 3| | | | | |
         *  -----------
         *  -----------
         * 2| | | | | |
         *  -----------
         *  -----------
         * 1| | | | | |
         *  -----------
         *  -----------
         * 0| | | | | |
         *  -----------
         *   0 1 2 3 4
         */
        private ChunkContainer[,] _chunkMap;
        private List<ChunkContainer> _freeContainerList = new List<ChunkContainer>();

        private readonly List<ChunkContainer> _chunkContainerPool = new List<ChunkContainer>();
        private readonly ConcurrentDictionary<ChunkPos, ChunkContainer> _chunkDict = new ConcurrentDictionary<ChunkPos, ChunkContainer>();

        private bool _isLoadingTask;

        public WorldInfo worldInfo;

        public WorldLoader worldLoader;
        public WorldGenerator worldGenerator { get; private set; }

        public EntityContainer globalEntityContainer;

        void Awake()
        {
            worldInfo = FindObjectOfType<WorldInfo>();

            if (worldInfo == null)
            {
                if (!IsWorldExists(DebugWorldName))
                {
                    CreateWorld(DebugWorldName, new ChunkPos(100, 100));
                    LoadWorld(DebugWorldName);
                }
                else
                    LoadWorld(DebugWorldName);
                enabled = false;
                return;
            }

            worldLoader.LoadWorld(worldInfo.worldPath);
            var worldGeneratorType = Type.GetType(worldInfo.worldGeneratorType);
            worldGenerator = (WorldGenerator) new GameObject().AddComponent(worldGeneratorType);
            EnlargePool(chunkPool);
        }

        private void Start()
        {
            globalEntityContainer.LoadAsync().Wait();
        }

        private void OnDisable()
        {
            if (globalEntityContainer.IsLoaded)
                globalEntityContainer.Unload();

            foreach (var chunk in _chunkContainerPool)
                chunk.Unload();
        }

        private void OnApplicationQuit()
        {
            enabled = false;
        }

        void Update()
        {
            UpdateChunk();
        }

        void UpdateChunk()
        {
            if (worldAnchor == null)
                return;

            if (_chunkMap == null || _chunkMap.Length != (int) Mathf.Pow(sight * 2 + 1, 2))
                _chunkMap = new ChunkContainer[sight * 2 + 1, sight * 2 + 1];

            var centerPos = worldAnchor.GetComponent<Entity>().ChunkPos;

            _freeContainerList.Clear();

            for (var x = 0; x < sight * 2 + 1; ++x)
            for (var y = 0; y < sight * 2 + 1; ++y)
                _chunkMap[x, y] = null;

            // 寻找在范围内的Chunk
            physicalReadyChunkCount = 0;

            foreach (var container in _chunkContainerPool)
            {
                if (!container.chunkPos.IsAvailable())
                {
                    _freeContainerList.Add(container);
                    continue;
                }

                var xDis = container.chunkPos.x - centerPos.x;
                var zDis = container.chunkPos.z - centerPos.z;

                if (Mathf.Abs(xDis) <= sight && Mathf.Abs(zDis) <= sight)
                {
                    _chunkMap[xDis + sight, zDis + sight] = container;
                    container.enabled = true;

                    if (container.IsPhysicsReady == true)
                        ++physicalReadyChunkCount;
                }
                else
                {
                    _freeContainerList.Add(container);
                    container.enabled = false;
                }
            }

            // 第一遍初始化是否完成
            if (physicalReadyChunkCount == _chunkMap.Length)
                IsInitializing = false;

            // 周围Chunk已经全部分配好
            if (_chunkContainerPool.Count - _freeContainerList.Count == _chunkMap.Length)
                return;

            // 正在加载chunk
            if (_isLoadingTask)
                return;

            // 排序
            _freeContainerList.Sort((container1, container2) =>
            {
                var dis1 = 
                    Mathf.Pow(container1.chunkPos.x - centerPos.x, 2) +
                    Mathf.Pow(container1.chunkPos.z - centerPos.z, 2);

                var dis2 =
                    Mathf.Pow(container2.chunkPos.x - centerPos.x, 2) +
                    Mathf.Pow(container2.chunkPos.z - centerPos.z, 2);

                if (!container1.chunkPos.IsAvailable() && container1.chunkPos.IsAvailable())
                    return -1;
                if (container1.chunkPos.IsAvailable() && !container1.chunkPos.IsAvailable())
                    return 1;
                if (dis1 > dis2)
                    return -1;
                if (dis1 < dis2)
                    return 1;
                if (container1.chunkPos.x > container2.chunkPos.x)
                    return -1;
                if (container1.chunkPos.x < container2.chunkPos.x)
                    return 1;
                if (container1.chunkPos.z > container2.chunkPos.z)
                    return -1;
                if (container1.chunkPos.z < container2.chunkPos.z)
                    return 1;
                return 0;
            });

            for (var x = 0; x < sight * 2 + 1; ++x)
                for (var z = 0; z < sight * 2 + 1; ++z)
                {
                    if (_chunkMap[x, z] != null)
                        continue;

                    if (_freeContainerList.Count == 0)
                        EnlargePool(1, ref _freeContainerList);
                
                    var freeContainer = _freeContainerList[0];
                    _chunkDict.TryRemove(freeContainer.chunkPos, out var _);
                    _freeContainerList.RemoveAt(0);

                    var chunkPos = new ChunkPos(centerPos.x + x - sight, centerPos.z + z - sight);

                    _isLoadingTask = true;
                    freeContainer.LoadAsync(chunkPos.x, chunkPos.z, () =>
                    {
                        _chunkDict[chunkPos] = freeContainer;
                        _isLoadingTask = false;
                    });
                    _chunkMap[x, z] = freeContainer;

                    // 非初始化时一次只加载一个Chunk
                    if (!IsInitializing)
                        return;
                }
        }

        void EnlargePool(int size)
        {
            var list = new List<ChunkContainer>();
            EnlargePool(size, ref list);
        }

        void EnlargePool(int size, ref List<ChunkContainer> containerList)
        {
            for (var i = 0; i < size; ++i)
            {
                var chunkObj = new GameObject();
                var container = chunkObj.AddComponent<ChunkContainer>();
                chunkObj.AddComponent<ChunkMeshGenerator>();
                chunkObj.AddComponent<MeshCollider>();

                chunkObj.transform.parent = chunkParent;
                container.enabled = false;
                container.SetSize(ChunkSize, BlockSize);

                _chunkContainerPool.Add(container);
                containerList.Add(container);
            }
        }

        public ChunkContainer GetChunk(ChunkPos chunkPos)
        {
            return !_chunkDict.TryGetValue(chunkPos, out var chunk) ? null : chunk;
        }

        /// <summary>
        /// 创建世界
        /// </summary>
        /// <param name="worldName"></param>
        /// <param name="worldSize"></param>
        /// <param name="onFinish"></param>
        public static void CreateWorld(string worldName, ChunkPos worldSize)
        {
            var worldGeneratorObj = new GameObject();
            var worldGenerator = worldGeneratorObj.AddComponent<NormalWorldGenerator>();

            worldGenerator.Generate(worldName, worldSize, new Vector3(1, 0.25f, 1), new Vector3Int(16, 128, 16));
            Destroy(worldGeneratorObj);
        }

        /// <summary>
        /// 加载世界
        /// </summary>
        /// <param name="worldName"></param>
        public static void LoadWorld(string worldPath)
        {
            var worldInfo = new GameObject("World info").AddComponent<WorldInfo>();
            worldInfo.worldPath = worldPath;
            DontDestroyOnLoad(worldInfo);

            SceneUtils.SwitchScene("Game/Scenes/Game");
        }

        public static bool IsWorldExists(string worldPath)
        {
            return Directory.Exists(Application.persistentDataPath + "/world/" + worldPath);
        }
    }
}
