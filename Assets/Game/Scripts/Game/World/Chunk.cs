using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Island.Game.Proxy.Blocks;
using Island.Game.EntityBehaviour;
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
    public class Chunk : MonoBehaviour
    {
        public ChunkPos ChunkPos { get; private set; } = ChunkPos.nonAvailable;

        private EntityContainer _entityContainer;
        private BlockContainer _blockContainer;

        public bool IsPhysicsReady => _blockContainer.IsPhysicsReady.GetValueOrDefault();

        private void Awake()
        {
            var entityContainerObj = new GameObject("Entity Container");
            var blockContainerObj = new GameObject("Block Container");

            entityContainerObj.transform.parent = transform;
            blockContainerObj.transform.parent = transform;

            _entityContainer = entityContainerObj.AddComponent<EntityContainer>();
            _blockContainer = blockContainerObj.AddComponent<BlockContainer>();
            blockContainerObj.AddComponent<ChunkMeshGenerator>();
            blockContainerObj.AddComponent<MeshCollider>();
        }

        public void SetSize(Vector3Int containerSize, Vector3 blockSize)
        {
            _blockContainer.SetSize(containerSize, blockSize);
        }

        public void AddEntity(Entity entity)
        {
            _entityContainer.Add(entity);
        }

        public Task AsyncLoadTask(ChunkPos chunkPos)
        {
            ChunkPos = chunkPos;

            var loadBlockTask = _blockContainer.AsyncLoadTask(ChunkPos);
            var loadEntityTask = _entityContainer.AsyncLoadTask(ChunkPos);

            var loadTask = new Task(() =>
            {
                loadBlockTask.Wait();
                loadEntityTask.Wait();
            });
            loadTask.Start();

#if UNITY_EDITOR
            name = "Chunk: " + ChunkPos.x + ", " + ChunkPos.z;
#endif
            transform.position = new Vector3(
                ChunkPos.x * _blockContainer.ContainerSize.x * _blockContainer.BlockSize.x, 
                0, 
                ChunkPos.z * _blockContainer.ContainerSize.z * _blockContainer.BlockSize.z);

            return loadTask;
        }

        public Task AsyncUnloadTask()
        {
            var unloadEntityTask = _entityContainer.AsyncUnloadTask();
            var unloadBlockTask = _blockContainer.AsyncUnloadTask();

            var unloadTask = new Task(() =>
            {
                unloadBlockTask.Wait();
                unloadEntityTask.Wait();
            });
            unloadTask.Start();

            return unloadTask;
        }

        void OnDisable()
        {
            _entityContainer.enabled = true;
            _blockContainer.enabled = false;
        }

        void OnEnable()
        {
            _entityContainer.enabled = true;
            _blockContainer.enabled = true;
        }
    }
}
