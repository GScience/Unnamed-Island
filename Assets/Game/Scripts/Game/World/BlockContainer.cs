using Island.Game.Data.Blocks;
using Island.Game.Render;
using Island.Game.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Island.Game.World
{
    public class BlockContainer : MonoBehaviour
    {
        public Vector3 BlockSize { get; private set; } = Vector3.zero;
        public Vector3Int ContainerSize { get; private set; } = Vector3Int.zero;

        private CancellationTokenSource _cts;
        private Task _genChunkTask;
        private Task _unloadTask;

        private IBlock _air;

        private Block[,,] _blocks;
        private bool _isDirty = true;

        public bool? IsPhysicsReady => _chunkMeshGenerator?.physicsReady;
        private ChunkMeshGenerator _chunkMeshGenerator;
        public ChunkPos ChunkPos { get; private set; } = ChunkPos.nonAvailable;

        public bool IsLoaded { get; private set; }

        public void Start()
        {
            _air = GameManager.DataManager.Get<IBlock>("island.block:air");
        }

        public void SetSize(Vector3Int containerSize, Vector3 blockSize)
        {
            _blocks = new Block[containerSize.x, containerSize.y, containerSize.z];
            ContainerSize = containerSize;
            BlockSize = blockSize;
        }

        public IBlock GetBlockData(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= ContainerSize.x || y >= ContainerSize.y || z >= ContainerSize.z)
                return _air;

            var block = _blocks[x, y, z];
            if (block.blockData == null)
                return _air;
            return block.blockData;
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

        public Task AsyncUnloadTask()
        {            
            // 卸载网格
            if (ChunkPos.IsAvailable())
                _chunkMeshGenerator?.Unload();

            if (_unloadTask != null && !_unloadTask.IsCompleted)
                return _unloadTask;

            IsLoaded = false;
            _isDirty = false;

            var needSave = IsLoaded;
            var oldChunkPos = ChunkPos;

            // 卸载Chunk
            _unloadTask = new Task(() =>
            {
                if (!oldChunkPos.IsAvailable())
                    return;

                if (needSave)
                    GameManager.WorldManager.worldLoader.SaveBlock(oldChunkPos, _blocks);

                for (var x = 0; x < ContainerSize.x; ++x)
                    for (var y = 0; y < ContainerSize.y; ++y)
                        for (var z = 0; z < ContainerSize.z; ++z)
                            _blocks[x, y, z].blockData = null;
            });
            _unloadTask.Start();

            return _unloadTask;
        }

        public Task AsyncLoadTask(ChunkPos chunkPos)
        {
            // 加载前结束之前此Chunk所有加载任务
            if (_genChunkTask != null && !_genChunkTask.IsCompleted)
                _cts.Cancel();

            // 卸载已经加载的区块
            if (_unloadTask == null || _unloadTask.IsCompleted)
                _unloadTask = AsyncUnloadTask();

            var oldChunkPos = ChunkPos;
            ChunkPos = chunkPos;

            _genChunkTask = new Task(() =>
            {
                // 等待卸载任务完成
                _unloadTask.Wait();

                _cts = new CancellationTokenSource();

                // 加载Chunk
                _genChunkTask = new Task(() =>
                {
                    GameManager.WorldManager.worldLoader.LoadBlock(chunkPos, ref _blocks);
                    IsLoaded = true;
                }, _cts.Token);

                // 等待区块加载完成
                _genChunkTask.Start();
                _genChunkTask.Wait();

                _isDirty = true;
            });

            _genChunkTask.Start();

            return _genChunkTask;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(BlockContainer))]
        class BlockContainerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                var container = (BlockContainer)serializedObject.targetObject;

                if (!GameManager.IsInitializing)
                    GUILayout.Label("Chunk: " + container.ChunkPos);
                base.OnInspectorGUI();
            }
        }
#endif
    }
}
