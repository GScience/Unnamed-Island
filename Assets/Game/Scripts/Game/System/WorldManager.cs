using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Controller;
using Game.Data.Block;
using Game.Render;
using Game.World;
using UnityEngine;

namespace Game.System
{
    public class WorldManager : MonoBehaviour
    {
        public Vector3 blockSize = Vector3.one;
        public Vector3Int chunkSize = new Vector3Int(16, 32, 16);
        public int chunkPool = 100;
        public Anchor worldAnchor;

        public int sight = 2;

        private IWorldGenerator _worldGenerator;
        public IWorldGenerator WorldGenerator
        {
            get => _worldGenerator;
            set
            {
                foreach (var chunkContainer in _chunkContainerPool)
                    chunkContainer.Unload();

                _worldGenerator = value;
            }
        }

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

        void Awake()
        {
            EnlargePool(chunkPool);
        }

        void Start()
        {
            WorldGenerator = new NormalWorldGenerator();
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
                }
                else
                {
                    _freeContainerList.Add(container);
                    container.enabled = false;
                }
            }

            // 已经全部加载完成
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
    }
}
