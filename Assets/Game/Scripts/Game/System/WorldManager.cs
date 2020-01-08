using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.EntityBehaviour;
using Island.Game.Proxy.Blocks;
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
        private const string DebugWorldName = "_DEBUG-WORLD_" + "200108.3";

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
        private Chunk[,] _chunkMap;
        /// <summary>
        /// 空闲区块列表
        /// </summary>
        private List<Chunk> _freeContainerList = new List<Chunk>();

        /// <summary>
        /// 区块池
        /// </summary>
        private readonly List<Chunk> _chunkContainerPool = new List<Chunk>();
        /// <summary>
        /// 区块字典，用于查摘区块
        /// </summary>
        private readonly ConcurrentDictionary<ChunkPos, Chunk> _chunkDict = new ConcurrentDictionary<ChunkPos, Chunk>();

        /// <summary>
        /// 是否有区块加载任务
        /// </summary>
        private Task _chunkLoadingTask;

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

        /// <summary>
        /// 实体材质
        /// </summary>
        public Material entityMaterial;

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
            globalEntityContainer.AsyncLoadTask(ChunkPos.global).Wait();
        }

        private void OnDisable()
        {
            if (globalEntityContainer.IsLoaded)
                globalEntityContainer.AsyncUnloadTask().Wait();

            foreach (var chunk in _chunkContainerPool)
                chunk.AsyncUnloadTask().Wait();
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
                _chunkMap = new Chunk[sight * 2 + 1, sight * 2 + 1];

            var centerPos = worldAnchor.GetComponent<Entity>().ChunkPos;

            _freeContainerList.Clear();

            for (var x = 0; x < sight * 2 + 1; ++x)
            for (var y = 0; y < sight * 2 + 1; ++y)
                _chunkMap[x, y] = null;

            // 寻找在范围内的Chunk
            _physicalReadyChunkCount = 0;

            foreach (var container in _chunkContainerPool)
            {
                if (!container.ChunkPos.IsAvailable())
                {
                    _freeContainerList.Add(container);
                    continue;
                }

                var xDis = container.ChunkPos.x - centerPos.x;
                var zDis = container.ChunkPos.z - centerPos.z;

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
            if (_chunkLoadingTask != null && !_chunkLoadingTask.IsCompleted)
                return;

            // 排序
            _freeContainerList.Sort((chunk1, chunk2) =>
            {
                var dis1 = 
                    Mathf.Pow(chunk1.ChunkPos.x - centerPos.x, 2) +
                    Mathf.Pow(chunk1.ChunkPos.z - centerPos.z, 2);

                var dis2 =
                    Mathf.Pow(chunk2.ChunkPos.x - centerPos.x, 2) +
                    Mathf.Pow(chunk2.ChunkPos.z - centerPos.z, 2);

                if (!chunk1.ChunkPos.IsAvailable() && chunk1.ChunkPos.IsAvailable())
                    return -1;
                if (chunk1.ChunkPos.IsAvailable() && !chunk1.ChunkPos.IsAvailable())
                    return 1;
                if (dis1 > dis2)
                    return -1;
                if (dis1 < dis2)
                    return 1;
                if (chunk1.ChunkPos.x > chunk2.ChunkPos.x)
                    return -1;
                if (chunk1.ChunkPos.x < chunk2.ChunkPos.x)
                    return 1;
                if (chunk1.ChunkPos.z > chunk2.ChunkPos.z)
                    return -1;
                if (chunk1.ChunkPos.z < chunk2.ChunkPos.z)
                    return 1;
                return 0;
            });

            // 一圈一圈加载
            for (var loopSight = 0; loopSight <= sight; ++loopSight)
            {
                var sightWidth = loopSight * 2;

                for (var i = 0; i <= (loopSight == 0 ? 1 : sightWidth * 4); ++i)
                {
                    var x = sight - loopSight;
                    var z = sight - loopSight;

                    if (i < sightWidth)
                    {
                        x += i;
                    }
                    else if (sightWidth <= i && i < sightWidth * 2)
                    {
                        x += sightWidth;
                        z += i - sightWidth;
                    }
                    else if (sightWidth * 2 <= i && i < sightWidth * 3)
                    {
                        x += 3 * sightWidth - i;
                        z += sightWidth;
                    }
                    else if (sightWidth * 3 <= i)
                    {
                        z += sightWidth * 4 - i;
                    }

                    if (_chunkMap[x, z] != null)
                        continue;

                    if (_freeContainerList.Count == 0)
                        EnlargePool(1, ref _freeContainerList);

                    var freeContainer = _freeContainerList[0];
                    _chunkDict.TryRemove(freeContainer.ChunkPos, out var _);
                    _freeContainerList.RemoveAt(0);

                    var chunkPos = new ChunkPos(centerPos.x + x - sight, centerPos.z + z - sight);

                    _chunkLoadingTask = freeContainer.AsyncLoadTask(chunkPos);

                    _chunkMap[x, z] = freeContainer;
                    _chunkDict[chunkPos] = freeContainer;

                    // 非初始化时一次只加载一个Chunk
                    /*if (!IsInitializing)*/
                    return;
                }
            }
        }

        void EnlargePool(int size)
        {
            var list = new List<Chunk>();
            EnlargePool(size, ref list);
        }

        void EnlargePool(int size, ref List<Chunk> containerList)
        {
            for (var i = 0; i < size; ++i)
            {
                var chunkObj = new GameObject();
                var chunk = chunkObj.AddComponent<Chunk>();

                chunkObj.transform.parent = chunkParent;
                chunk.enabled = false;
                chunk.SetSize(ChunkSize, BlockSize);

                _chunkContainerPool.Add(chunk);
                containerList.Add(chunk);
            }
        }

        public Chunk GetChunk(ChunkPos chunkPos)
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
