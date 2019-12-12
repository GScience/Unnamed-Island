using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Game.Data.Block;
using Game.Render;
using Game.System;
using UnityEngine;

namespace Game.World
{
    public class ChunkContainer : MonoBehaviour
    {
        public Vector3 BlockSize { get; private set; } = Vector3.zero;
        public Vector3Int ChunkSize { get; private set; } = Vector3Int.zero;

        public bool? PhysicsReady => _chunkMeshGenerator?.physicsReady;

        private ChunkMeshGenerator _chunkMeshGenerator;
        private Block[,,] _blocks;
        private bool _isDirty = true;

        private IBlock _air;

        public ChunkPos chunkPos = ChunkPos.nonAvailable;

        private CancellationTokenSource _cts;
        private Task _genChunkTask;

        public IBlock GetBlockData(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= ChunkSize.x || y >= ChunkSize.y || z >= ChunkSize.z)
                return _air;

            var block = _blocks[x, y, z];
            if (block.blockData == null)
                return _air;
            return block.blockData;
        }

        public void SetSize(Vector3Int chunkSize, Vector3 blockSize)
        {
            _blocks = new Block[chunkSize.x, chunkSize.y, chunkSize.z];
            ChunkSize = chunkSize;
            BlockSize = blockSize;
        }

        public void Start()
        {
            _air = GameManager.DataManager.Get<IBlock>("island.block:air");
        }
        public void SetBlock(int x, int y, int z, string blockName)
        {
            var data = GameManager.DataManager.Get<IBlock>(blockName);
            _blocks[x, y, z].blockData = data;
        }

        void LateUpdate()
        {
            if (!_isDirty)
                return;

            if (_chunkMeshGenerator == null)
                _chunkMeshGenerator = GetComponent<ChunkMeshGenerator>();

            if (_chunkMeshGenerator == null)
            {
                _isDirty = false;
                return;
            }

            if (_chunkMeshGenerator.TryRefresh())
                _isDirty = false;
        }

        void OnDisable()
        {
            if (_chunkMeshGenerator == null)
                _chunkMeshGenerator = GetComponent<ChunkMeshGenerator>();

            if (_chunkMeshGenerator != null) 
                _chunkMeshGenerator.enabled = false;
        }

        void OnEnable()
        {
            if (_chunkMeshGenerator == null)
                _chunkMeshGenerator = GetComponent<ChunkMeshGenerator>();

            if (_chunkMeshGenerator != null)
                _chunkMeshGenerator.enabled = true;
        }

        public void Unload()
        {
            if (!chunkPos.IsAvailable())
                return;

            _chunkMeshGenerator.Unload();
            chunkPos = ChunkPos.nonAvailable;

            for (var x = 0; x < ChunkSize.x; ++x)
            for (var y = 0; y < ChunkSize.y; ++y)
            for (var z = 0; z < ChunkSize.z; ++z)
                _blocks[x, y, z].blockData = null;
        }

        public void Load(int chunkX, int chunkZ)
        {
            // 加载前尝试卸载
            Unload();

            chunkPos.x = chunkX;
            chunkPos.z = chunkZ;

            GameManager.WorldManager.WorldGenerator.GenChunk(chunkPos, ref _blocks);

            name = "Chunk: " + chunkX + ", " + chunkZ;
            transform.position = new Vector3(chunkX * ChunkSize.x * BlockSize.x, 0, chunkZ * ChunkSize.z * BlockSize.z);

            _isDirty = true;
        }

        public async void LoadAsync(int chunkX, int chunkZ, Action onFinished = null)
        {
            // 加载前尝试卸载
            Unload();

            chunkPos.x = chunkX;
            chunkPos.z = chunkZ;

            name = "Chunk: " + chunkX + ", " + chunkZ;
            transform.position = new Vector3(chunkX * ChunkSize.x * BlockSize.x, 0, chunkZ * ChunkSize.z * BlockSize.z);

            if (_genChunkTask != null && !_genChunkTask.IsCompleted)
                _cts.Cancel();

            _cts = new CancellationTokenSource();

            _genChunkTask = new Task(() => GameManager.WorldManager.WorldGenerator.GenChunk(chunkPos, ref _blocks), _cts.Token);
            _genChunkTask.Start();
            await _genChunkTask;

            _isDirty = true;
            onFinished?.Invoke();
        }
    }
}
