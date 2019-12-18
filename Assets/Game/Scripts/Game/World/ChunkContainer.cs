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

        public EntityContainer EntityContainer { get; private set; }

        public bool IsLoaded { get; private set; }

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

        private EntityContainer CreateEntityContainer()
        {
            var entityContainerObj = new GameObject("Entity Container");
            entityContainerObj.transform.parent = transform;
            return entityContainerObj.AddComponent<EntityContainer>();
        }

        private void Awake()
        {
            EntityContainer = CreateEntityContainer();
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

            EntityContainer.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            if (_chunkMeshGenerator == null)
                _chunkMeshGenerator = GetComponent<ChunkMeshGenerator>();

            if (_chunkMeshGenerator != null)
                _chunkMeshGenerator.enabled = true;

            EntityContainer.gameObject.SetActive(true);
        }

        public Task UnloadAndSetChunkPosTask(ChunkPos newPos)
        {
            if (_unloadTask != null && !_unloadTask.IsCompleted)
            {
                chunkPos = newPos;
                return _unloadTask;
            }

            var needSave = IsLoaded;

            IsLoaded = false;
            _isDirty = false;

            // 保存实体
            Task unloadEntityTask = null;

            // 卸载网格
            if (chunkPos.IsAvailable())
            {
                _chunkMeshGenerator.Unload();
                unloadEntityTask = EntityContainer.AsyncUnloadTask();
            }

            var oldPos = chunkPos;
            chunkPos = newPos;

            // 卸载Chunk
            var unloadChunkTask = new Task(() =>
            {
                if (!oldPos.IsAvailable())
                    return;

                // 等待区块加载完成
                _genChunkTask?.Wait();

                if (needSave)
                    GameManager.WorldManager.worldLoader.SaveChunkBlock(oldPos, _blocks);

                for (var x = 0; x < ChunkSize.x; ++x)
                    for (var y = 0; y < ChunkSize.y; ++y)
                        for (var z = 0; z < ChunkSize.z; ++z)
                            _blocks[x, y, z].blockData = null;

                // 等待实体卸载完成
                unloadEntityTask?.Wait();
            });
            unloadChunkTask.Start();
            _unloadTask = unloadChunkTask;
            return unloadChunkTask;
        }

        public void Unload()
        {
            _genChunkTask?.Wait();

            if (_unloadTask == null || _unloadTask.IsCompleted)
                UnloadAndSetChunkPosTask(ChunkPos.nonAvailable).Wait();
            else
                _unloadTask.Wait();
        }

        public void Load(int chunkX, int chunkZ)
        {
            // 加载前卸载当前加载的Chunk
            Unload();

            chunkPos.x = chunkX;
            chunkPos.z = chunkZ;

            // 加载区块
            GameManager.WorldManager.worldLoader.LoadChunkBlock(chunkPos, ref _blocks);

            // 加载实体
            EntityContainer.AsyncLoadTask().Wait();

            name = "Chunk: " + chunkX + ", " + chunkZ;
            transform.position = new Vector3(chunkX * ChunkSize.x * BlockSize.x, 0, chunkZ * ChunkSize.z * BlockSize.z);

            _isDirty = true;
            IsLoaded = true;
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

            // 等待卸载任务完成
            await _unloadTask;

            _cts = new CancellationTokenSource();

            // 加载Chunk
            _genChunkTask = new Task(() =>
            {
                GameManager.WorldManager.worldLoader.LoadChunkBlock(chunkPos, ref _blocks);
                IsLoaded = true;
            }, _cts.Token);

            _genChunkTask.Start();

            // 等待区块加载完成
            await _genChunkTask;

            // 等待实体加载完成
            await EntityContainer.AsyncLoadTask();

            _isDirty = true;
            onFinished?.Invoke();
        }
    }
}
