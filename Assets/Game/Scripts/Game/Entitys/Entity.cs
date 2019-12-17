using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.System;
using Island.Game.World;
using UnityEditor;
using UnityEngine;

namespace Island.Game.Entitys
{
    public abstract class Entity : MonoBehaviour
    {
        public ChunkPos ChunkPos => new ChunkPos(
            Mathf.FloorToInt(transform.position.x / 
                (GameManager.WorldManager.ChunkSize.x * GameManager.WorldManager.BlockSize.x)), 
            Mathf.FloorToInt(transform.position.z / 
                (GameManager.WorldManager.ChunkSize.z * GameManager.WorldManager.BlockSize.z)));

        public EntityContainer Owner { get; private set; }
        protected EntityData _entityData;

        public ChunkContainer GetChunk(ChunkPos chunkPos)
        {
            return GameManager.WorldManager.GetChunk(chunkPos);
        }

        public void SetEntityData(EntityData entityData)
        {
            if (entityData.EntityType != GetType().FullName)
                Debug.LogError("Wrong entity data set to entity");
            _entityData = entityData;

            LoadFromEntityData();
        }

        public EntityData GetEntityData()
        {
            if (_entityData == null)
                _entityData = EntityData.Empty;
            SaveToEntityData();

            return _entityData;
        }

        private void Start()
        {
            Owner = transform.parent?.GetComponent<EntityContainer>();
        }
        void Update()
        {
            // 初始化时不刷新实体
            if (GameManager.IsInitializing)
                return;

            if (transform.hasChanged)
            {
                transform.hasChanged = false;

                // 刷新实体所有者
                UpdateOwner();

                // 所在Chunk无效时不刷新实体
                var chunk = GetChunk(ChunkPos);
                if (chunk == null || chunk.IsPhysicsReady != true)
                    return;
            }

            // 刷新实体移动
            UpdateMovement();
        }

        void UpdateOwner()
        {
            if (Owner == null)
                Debug.LogError("Each entity should belong to a container");

            if (Owner.IsGlobalContainer)
                return;

            // 刷新实体所有权
            if (ChunkPos != Owner.chunkPos)
            {
                var chunk = GameManager.WorldManager.GetChunk(ChunkPos);
                if (chunk != null)
                {
                    Owner.Remove(this);
                    Owner = null;
                    chunk.EntityContainer.Add(this);
                }

                // 更改所有权
                Owner = transform.parent?.GetComponent<EntityContainer>();
            }
        }

        protected abstract void UpdateMovement();

        protected virtual void SaveToEntityData()
        {
            _entityData.Set("type", GetType().FullName);
            _entityData.Set("name", gameObject.name);
            _entityData.Set("position", transform.position);
        }

        protected virtual void LoadFromEntityData()
        {
            transform.position = _entityData.TryGet("position", transform.position);
            gameObject.name = _entityData.Get<string>("name");
        }

        public static T Create<T>(EntityContainer container, string entityName = null) where T : Entity
        {
            if (container.IsGlobalContainer)
                Debug.LogError("Can't create entity in a global entity container");

            return (T) Create(container, typeof(T), entityName);
        }

        public static Entity Create(EntityContainer container, Type type, string entityName = null)
        {
            var entityObj = new GameObject(entityName == null ? "Entity" : entityName);
            var entity = entityObj.AddComponent(type);
            container.Add((Entity) entity);
            return (Entity) entity;
        }
#if UNITY_EDITOR
        [CustomEditor(typeof(Entity),editorForChildClasses:true)]
        class EntityEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                var entity = (Entity) serializedObject.targetObject;

                if (!GameManager.IsInitializing)
                    GUILayout.Label("In Chunk: " + entity.ChunkPos.x + entity.ChunkPos.z);
                base.OnInspectorGUI();
            }
        }
#endif
    }
}
