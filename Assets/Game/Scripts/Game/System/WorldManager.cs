using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Controller;
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
        /// <summary>
        /// 标记是否在初始化（第一次初始化所有Chunk状态）
        /// </summary>
        public bool IsInitializing { get; private set; }

        public Vector3 blockSize = Vector3.one;
        public Vector3Int chunkSize = new Vector3Int(16, 32, 16);
        public int chunkPool = 100;
        public Anchor worldAnchor;

        public int sight = 2;

        public Transform chunkParent;

        public int physicalReadyChunkCount = 0;

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
        private readonly Dictionary<ChunkPos, ChunkContainer> _chunkDict = new Dictionary<ChunkPos, ChunkContainer>();

        private WorldInfo worldInfo;

        public WorldLoader worldLoader;

        void Awake()
        {
            worldInfo = FindObjectOfType<WorldInfo>();

#if UNITY_EDITOR
            if (worldInfo == null)
            {
                if (!IsWorldExists("_DEBUG-WORLD_"))
                    CreateWorld("_DEBUG-WORLD_", new ChunkPos(20, 20), () => LoadWorld("_DEBUG-WORLD_"));
                else
                    LoadWorld("_DEBUG-WORLD_");
                enabled = false;
                return;
            }
#endif
            worldLoader.LoadWorld(worldInfo.worldPath);
            EnlargePool(chunkPool);
        }

        void Start()
        {
            IsInitializing = true;
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

            var centerPos = worldAnchor.GetComponent<Character>().ChunkPos;

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
                _chunkDict.Remove(freeContainer.chunkPos);
                _freeContainerList.RemoveAt(0);
                
                var chunkPos = new ChunkPos(centerPos.x + x - sight, centerPos.z + z - sight);
                freeContainer.LoadAsync(chunkPos.x, chunkPos.z, () => _chunkDict[chunkPos] = freeContainer);
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
                container.SetSize(chunkSize, blockSize);

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
        public static void CreateWorld(string worldName, ChunkPos worldSize, Action onFinish = null)
        {
            var pannel = Pannel.Show("WorldCreating");
            var worldCreatingPannel = pannel.GetComponent<WorldCreatingPannel>();

            var worldGeneratorObj = new GameObject();
            var worldGenerator = worldGeneratorObj.AddComponent<NormalWorldGenerator>();

            worldGenerator.onStageChange += (string stage) =>
            {
                worldCreatingPannel.SetStage(stage);
            };
            worldGenerator.onLoaded += () =>
            {
                pannel.Close();
                onFinish();
            };

            worldGenerator.Generate(worldName, worldSize, new Vector3(1, 0.25f, 1), new Vector3Int(16, 128, 16));
        }

        /// <summary>
        /// 加载世界
        /// </summary>
        /// <param name="worldName"></param>
        public static void LoadWorld(string worldPath)
        {
            var worldInfo = new GameObject("World info").AddComponent<WorldInfo>();
            worldInfo.worldPath = worldPath;
            worldInfo.chunkCount = new ChunkPos(20, 20);
            worldInfo.chunkSize = new Vector3Int(16, 128, 16);
            DontDestroyOnLoad(worldInfo);

            SceneUtils.SwitchScene("Game/Scenes/Game");
        }

        public static bool IsWorldExists(string worldPath)
        {
            return Directory.Exists(Application.persistentDataPath + "/world/" + worldPath);
        }
    }
}
