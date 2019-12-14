using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Island.Game.Data.Blocks;
using Island.Game.Render;
using Island.Game.System;
using Unity.Jobs;
using UnityEngine;

namespace Island.Game.World
{
    /// <summary>
    /// 区块容器
    /// 用来在游戏中储存区块信息
    /// </summary>
    public class ChunkContainer : MonoBehaviour
    {
        private static ConcurrentDictionary<ChunkPos, object> _loadedChunkLock 
            = new ConcurrentDictionary<ChunkPos, object>();

        public Vector3 BlockSize { get; private set; } = Vector3.zero;
        public Vector3Int ChunkSize { get; private set; } = Vector3Int.zero;

        public bool? IsPhysicsReady => _chunkMeshGenerator?.physicsReady;

        private ChunkMeshGenerator _chunkMeshGenerator;
        private Block[,,] _blocks;
        private bool _isDirty = true;
        
        private IBlock _air;

        public ChunkPos chunkPos = ChunkPos.nonAvailable;

        private CancellationTokenSource _cts;
        private Task _genChunkTask;
        private Task _unloadTask;

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

        private void OnDestroy()
        {
            // 如果正在异步卸载则直接等待异步卸载
            if (_unloadTask == null || _unloadTask.IsCompleted)
                Unload();
            else
                _unloadTask.Wait();
        }

        public Task UnloadAndSetChunkPosTask(ChunkPos newPos)
        {
            var oldPos = chunkPos;
            chunkPos = newPos;

            if (chunkPos.IsAvailable())
                _chunkMeshGenerator.Unload();

            var task = new Task(() =>
            {
                if (!oldPos.IsAvailable())
                    return;

                lock (_loadedChunkLock[oldPos])
                    GameManager.WorldManager.worldLoader.SaveChunk(oldPos, _blocks);

                for (var x = 0; x < ChunkSize.x; ++x)
                    for (var y = 0; y < ChunkSize.y; ++y)
                        for (var z = 0; z < ChunkSize.z; ++z)
                            _blocks[x, y, z].blockData = null;

                if (!_loadedChunkLock.TryRemove(oldPos, out var _))
                    Debug.LogError("Failed to remove chunk");
            });
            task.Start();
            return task;
        }

        public void Unload()
        {
            if (!chunkPos.IsAvailable())
                return;

            _chunkMeshGenerator.Unload();

            lock (_loadedChunkLock[chunkPos])
                GameManager.WorldManager.worldLoader.SaveChunk(chunkPos, _blocks);

            if (!_loadedChunkLock.TryRemove(chunkPos, out var _))
                Debug.LogError("Failed to remove chunk");

            chunkPos = ChunkPos.nonAvailable;

            for (var x = 0; x < ChunkSize.x; ++x)
                for (var y = 0; y < ChunkSize.y; ++y)
                    for (var z = 0; z < ChunkSize.z; ++z)
                        _blocks[x, y, z].blockData = null;
        }

        public void Load(int chunkX, int chunkZ)
        {
            // 加载前卸载当前加载的Chunk
            Unload();

            chunkPos.x = chunkX;
            chunkPos.z = chunkZ;

            object chunkLock;

            if (!_loadedChunkLock.TryGetValue(chunkPos, out chunkLock))
            {
                chunkLock = new object();
                _loadedChunkLock[chunkPos] = chunkLock;
            }

            lock (chunkLock)
                GameManager.WorldManager.worldLoader.LoadChunk(chunkPos, ref _blocks);

            name = "Chunk: " + chunkX + ", " + chunkZ;
            transform.position = new Vector3(chunkX * ChunkSize.x * BlockSize.x, 0, chunkZ * ChunkSize.z * BlockSize.z);

            _isDirty = true;
        }

        public async void LoadAsync(int chunkX, int chunkZ, Action onFinished = null)
        {
            // 加载前结束之前此Chunk所有加载任务
            if (_genChunkTask != null && !_genChunkTask.IsCompleted)
                _cts.Cancel();

            if (_unloadTask == null || _unloadTask.IsCompleted)
            {
                // 在卸载区块的时候设置新区块坐标
                _unloadTask = UnloadAndSetChunkPosTask(new ChunkPos(chunkX, chunkZ));
            }
            else
            {
                // 直接设置区块坐标
                chunkPos.x = chunkX;
                chunkPos.z = chunkZ;
            }

            // 区块基本信息
            name = "Chunk: " + chunkX + ", " + chunkZ;
            transform.position = new Vector3(chunkX * ChunkSize.x * BlockSize.x, 0, chunkZ * ChunkSize.z * BlockSize.z);

            // 获取区块锁
            object chunkLock;

            if (!_loadedChunkLock.TryGetValue(chunkPos, out chunkLock))
            {
                chunkLock = new object();
                _loadedChunkLock[chunkPos] = chunkLock;
            }

            // 等待卸载任务完成
            await _unloadTask;

            _cts = new CancellationTokenSource();

            // 加载Chunk
            _genChunkTask = new Task(() =>
            {
                lock (chunkLock)
                    GameManager.WorldManager.worldLoader.LoadChunk(chunkPos, ref _blocks);

                _isDirty = true;
                onFinished?.Invoke();
            }, _cts.Token);

            _genChunkTask.Start();
            await _genChunkTask;
        }
    }
}
