using Island.Game.Entitys;
using Island.Game.System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Island.Game.World
{
    /// <summary>
    /// 实体容器
    /// 世界管理器负责管理管理全局实体容器
    /// Chunk会各自管理与其关联的实体容器
    /// 除全局实体容器外，其他容器都不会由世界管理器直接控制
    /// </summary>
    public class EntityContainer : MonoBehaviour
    {
        public bool IsLoaded { get; private set; }

        private List<Entity> _entityList = new List<Entity>();
        private List<EntityData> _entityDataList = new List<EntityData>();

        private bool _isDirty;

        private static int _entityUpdationCotourine = 0;

        private Task _unloadTask;
        private Task _loadTask;
        private CancellationTokenSource _cts;

        public ChunkPos ChunkPos { get; private set; } = ChunkPos.nonAvailable;

        public bool IsGlobalContainer => ChunkPos.IsGlobal();

        private bool _hasLoadingCoroutine = false;

        private void Awake()
        {
            // 加载实体容器中的默认实体
            foreach (var entity in GetComponentsInChildren<Entity>())
            {
#if UNITY_EDITOR
                foreach (var findEntity in _entityList)
                    if (findEntity.gameObject.name == entity.gameObject.name)
                        Debug.LogError("There is two entity in global container with the same name");
#endif
                Add(entity);
            }
        }

        public Task AsyncLoadTask(ChunkPos chunkPos)
        {
            if (_unloadTask == null || _unloadTask.IsCompleted)
                _unloadTask = AsyncUnloadTask();

            var oldChunkPos = ChunkPos;
            ChunkPos = chunkPos;

            if (_loadTask != null && !_loadTask.IsCompleted)
                _cts.Cancel();

            _cts = new CancellationTokenSource();

            var loadTask = new Task(() =>
            {
                _unloadTask.Wait();
                GameManager.WorldManager.worldLoader.LoadEntity(chunkPos, ref _entityDataList);
                _isDirty = true;
            }, _cts.Token);
            loadTask.Start();
            _loadTask = loadTask;
            return loadTask;
        }

        public Task AsyncUnloadTask()
        {
            if (_unloadTask != null && !_unloadTask.IsCompleted)
                return _unloadTask;

            var needSave = IsLoaded;
            var oldChunkPos = ChunkPos;

            IsLoaded = false;
            _isDirty = false;

            foreach (var entity in _entityList)
                _entityDataList.Add(entity.GetEntityData());

            if (!IsGlobalContainer && ChunkPos.IsAvailable())
            {
                foreach (var entity in _entityList)
                    Destroy(entity.gameObject);

                _entityList.Clear();
            }

            StopAllCoroutines();
            if (_hasLoadingCoroutine)
                --_entityUpdationCotourine;

            _unloadTask = new Task(() =>
            {
                if (needSave)
                    GameManager.WorldManager.worldLoader.SaveEntity(oldChunkPos, _entityDataList);
                _entityDataList.Clear();
            });
            _unloadTask.Start();

            return _unloadTask;
        }

        public List<Entity> GetEntityList()
        {
            return _entityList;
        }

        public Entity Create(Type type, string entityName = null)
        {
            return Entity.Create(this, type, entityName);
        }

        public Entity Create<T>() where T: Entity
        {
            return Entity.Create<T>(this);
        }

        public void Add(Entity entity)
        {
            if (entity.Owner != null)
                Debug.LogError("Entity has already in another container");
            else
            {
                _entityList.Add(entity);
                entity.transform.parent = transform;
            }
        }

        public void Remove(Entity entity)
        {
            if (!_entityList.Remove(entity))
            {
                Debug.LogError("Entity not found in container");
            }
            else
                entity.transform.parent = null;
        }

        IEnumerator DirtyContainerUpdateCorutine()
        {
            _isDirty = false;
            ++_entityUpdationCotourine;
            _hasLoadingCoroutine = true;

            var entityAddCountInFrame = 0;

            if (IsGlobalContainer)
            {
                // 全局容器直接同步数据
                foreach (var entityData in _entityDataList)
                {
                    var entity = _entityList.Find((Entity e) => e.gameObject.name == entityData.EntityName);
                    entity.SetEntityData(entityData);
                }
            }
            else
            {
                // 非全局容器直接创建对象
                foreach (var entityData in _entityDataList)
                {
                    var entityTypeName = entityData.EntityType;
                    var entityType = Type.GetType(entityTypeName);
                    var entity = Create(entityType);
                    entity.SetEntityData(entityData);

                    if (++entityAddCountInFrame > 1)
                    {
                        entityAddCountInFrame = 0;
                        yield return 1;
                    }
                }
            }

            _entityDataList.Clear();
            --_entityUpdationCotourine;
            _hasLoadingCoroutine = false;

            IsLoaded = true;
        }

        void Update()
        {
            if (_isDirty)
            {
                if (_entityUpdationCotourine < 1)
                    StartCoroutine(DirtyContainerUpdateCorutine());
            }
        }

        void OnDisable()
        {
            foreach (var entity in _entityList)
                entity.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            foreach (var entity in _entityList)
                entity.gameObject.SetActive(true);
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(EntityContainer), editorForChildClasses: true)]
        class EntityContainerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                var container = (EntityContainer) serializedObject.targetObject;

                if (!GameManager.IsInitializing)
                    GUILayout.Label("Entitys (" + container._entityList.Count + ")");
                base.OnInspectorGUI();
            }
        }
#endif
    }
}
