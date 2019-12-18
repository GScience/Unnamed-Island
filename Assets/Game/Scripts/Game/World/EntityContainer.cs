using Island.Game.Entitys;
using Island.Game.System;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private ChunkContainer chunkContainer;

        public bool IsLoaded { get; private set; }

        public bool IsGlobalContainer => chunkContainer == null;
        public ChunkPos chunkPos => chunkContainer?.chunkPos ?? ChunkPos.nonAvailable;

        private List<Entity> _entityList = new List<Entity>();
        private List<EntityData> _entityDataList = new List<EntityData>();

        private bool _isDirty;

        private static int _entityUpdationCotourine = 0;

        private void Awake()
        {
            chunkContainer = GetComponentInParent<ChunkContainer>();

            // 加载全局实体容器中的默认实体
            if (IsGlobalContainer)
            {
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
        }

        public Task LoadAsync()
        {
            var pos = chunkPos;

            var saveTask = new Task(() =>
            {
                GameManager.WorldManager.worldLoader.LoadEntity(pos, ref _entityDataList);
                _isDirty = true;
            });
            saveTask.Start();
            return saveTask;
        }

        public void Unload()
        {
            foreach (var entity in _entityList)
                _entityDataList.Add(entity.GetEntityData());

            foreach (var entity in _entityList)
                Destroy(entity.gameObject);

            GameManager.WorldManager.worldLoader.SaveEntity(chunkPos, _entityDataList);

            _entityList.Clear();
            _entityDataList.Clear();

            StopAllCoroutines();
            _entityUpdationCotourine = 0;
        }

        public Task UnloadAsync()
        {
            foreach (var entity in _entityList)
                _entityDataList.Add(entity.GetEntityData());

            foreach (var entity in _entityList)
                Destroy(entity.gameObject);

            StopAllCoroutines();

            _entityList.Clear();

            var pos = chunkPos;

            var saveTask = new Task(() =>
            {
                GameManager.WorldManager.worldLoader.SaveEntity(pos, _entityDataList);
                _entityDataList.Clear();
            });
            saveTask.Start();
            return saveTask;
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
            IsLoaded = true;
            _isDirty = false;
            ++_entityUpdationCotourine;

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

                    if (++entityAddCountInFrame > 3)
                    {
                        entityAddCountInFrame = 0;
                        yield return 1;
                    }
                }
            }

            _entityDataList.Clear();
            --_entityUpdationCotourine;
        }

        void Update()
        {
            if (_isDirty)
            {
                if (_entityUpdationCotourine < 1)
                    StartCoroutine(DirtyContainerUpdateCorutine());
            }
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
