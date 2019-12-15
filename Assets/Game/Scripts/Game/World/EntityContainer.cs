using Island.Game.Entitys;
using Island.Game.System;
using System;
using System.Collections.Generic;
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
        public ChunkPos chunkPos => chunkContainer.chunkPos;

        private List<Entity> _entityList = new List<Entity>();

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
                    _entityList.Add(entity);
                }
            }
        }

        public List<Entity> GetEntityList()
        {
            return _entityList;
        }

        public Entity Add(Type type, string entityName = null)
        {
            return Entity.Create(this, type, entityName);
        }

        public Entity Add<T>() where T: Entity
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
                Debug.LogError("Entity not found in container");
            else
                entity.transform.parent = null;
        }

        public void SaveToEntityData()
        {
            foreach (var entity in _entityList)
                entity.SaveToEntityData();
        }

        public void Refresh()
        {
            IsLoaded = true;

            foreach (var entity in _entityList)
                entity.Refresh();
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
