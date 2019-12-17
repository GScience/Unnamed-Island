using Island.Game.Entitys;
using Island.Game.System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Island.Game.World
{
    /// <summary>
    /// 实体容器
    /// 全局实体容器由WorldManager管理，Chunk实体容器由ChunkContainer管理
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

            GameManager.WorldManager.worldLoader.SaveEntity(chunkPos, _entityDataList);

            foreach (var entity in _entityList)
                Destroy(entity.gameObject);
            _entityList.Clear();
            _entityDataList.Clear();
        }

        public Task UnloadAsync()
        {
            foreach (var entity in _entityList)
                _entityDataList.Add(entity.GetEntityData());

            foreach (var entity in _entityList)
                Destroy(entity.gameObject);

            _entityList.Clear();
            _entityDataList.Clear();

            var pos = chunkPos;

            var saveTask = new Task(() =>
            {
                GameManager.WorldManager.worldLoader.SaveEntity(pos, _entityDataList);
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
                Debug.DebugBreak();
                Debug.LogError("Entity not found in container");
            }
            else
                entity.transform.parent = null;
        }

        public void Update()
        {
            if (!_isDirty)
                return;

            IsLoaded = true;

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
                }
            }

            _entityDataList.Clear();

            _isDirty = false;
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
