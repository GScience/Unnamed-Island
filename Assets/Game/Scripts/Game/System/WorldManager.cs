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
    /// 负责自动加载Chunk等
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        private const string DebugWorldName = "_DEBUG-WORLD_" + "191218.1";

        /// <summary>
        /// 标记是否在初始化（第一次初始化所有Chunk状态）
        /// </summary>
        public bool IsInitializing { get; private set; } = true;

        /// <summary>
        /// 方块的大小
        /// </summary>
        public Vector3 BlockSize => worldInfo.blockSize;
        /// <summary>
        /// 区块的大小
        /// </summary>
        public Vector3Int ChunkSize => worldInfo.chunkSize;

        /// <summary>
        /// 区块池
        /// </summary>
        public int chunkPool = 100;
        /// <summary>
        /// 世界锚
        /// </summary>
        public Anchor worldAnchor;

        /// <summary>
        /// 视野
        /// </summary>
        public int sight = 2;

        /// <summary>
        /// 创建的区块容器的父对象
        /// </summary>
        public Transform chunkParent;

        /// <summary>
        /// 物理计算完成的区块数量
        /// </summary>
        private int _physicalReadyChunkCount = 0;

        /// <summary>
        /// 区块图，记录使用的区块
        /// </summary>
        private ChunkContainer[,] _chunkMap;
        /// <summary>
        /// 空闲区块列表
        /// </summary>
        private List<ChunkContainer> _freeContainerList = new List<ChunkContainer>();

        /// <summary>
        /// 区块池
        /// </summary>
        private readonly List<ChunkContainer> _chunkContainerPool = new List<ChunkContainer>();
        /// <summary>
        /// 区块字典，用于查摘区块
        /// </summary>
        private readonly ConcurrentDictionary<ChunkPos, ChunkContainer> _chunkDict = new ConcurrentDictionary<ChunkPos, ChunkContainer>();

        /// <summary>
        /// 是否有区块加载任务
        /// </summary>
        private bool _hasLoadingTask;

        /// <summary>
        /// 世界信息
        /// </summary>
        public WorldInfo worldInfo;

        /// <summary>
        /// 世界加载器（为了支持老版本加入）
        /// </summary>
        public WorldLoader worldLoader;
        /// <summary>
        /// 世界生成器
        /// </summary>
        public WorldGenerator worldGenerator { get; private set; }

        /// <summary>
        /// 实体容器
        /// </summary>
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
            _physicalReadyChunkCount = 0;

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
                        ++_physicalReadyChunkCount;
                }
                else
                {
                    _freeContainerList.Add(container);
                    container.enabled = false;
                }
            }

            // 第一遍初始化是否完成
            if (_physicalReadyChunkCount == _chunkMap.Length)
                IsInitializing = false;

            // 周围Chunk已经全部分配好
            if (_chunkContainerPool.Count - _freeContainerList.Count == _chunkMap.Length)
                return;

            // 正在加载chunk
            if (_hasLoadingTask)
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

                    _hasLoadingTask = true;
                    freeContainer.LoadAsync(chunkPos.x, chunkPos.z, () =>
                    {
                        _chunkDict[chunkPos] = freeContainer;
                        _hasLoadingTask = false;
                    });
                    _chunkMap[x, z] = freeContainer;

                    // 非初始化时一次只加载一个Chunk
                    /*if (!IsInitializing)*/
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
