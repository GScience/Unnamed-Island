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
    /// <summary>
    /// 实体抽象类
    /// 所有实体需要继承此类
    /// </summary>
    public abstract class Entity : MonoBehaviour
    {
        /// <summary>
        /// 实体所在区块
        /// </summary>
        public ChunkPos ChunkPos => new ChunkPos(
            Mathf.FloorToInt(transform.position.x / 
                (GameManager.WorldManager.ChunkSize.x * GameManager.WorldManager.BlockSize.x)), 
            Mathf.FloorToInt(transform.position.z / 
                (GameManager.WorldManager.ChunkSize.z * GameManager.WorldManager.BlockSize.z)));

        /// <summary>
        /// 实体所有者（实体容器）
        /// </summary>
        public EntityContainer Owner { get; private set; }

        protected bool HasUpdation
        {
            get => enabled;
            set => enabled = value;
        }

        /// <summary>
        /// 实体数据
        /// </summary>
        protected EntityData _entityData;

        /// <summary>
        /// 是否可以移动
        /// </summary>
        private bool _canMove;

        /// <summary>
        /// 获取实体所在Chunk
        /// </summary>
        /// <param name="chunkPos"></param>
        /// <returns></returns>
        public ChunkContainer GetChunk()
        {
            return GameManager.WorldManager.GetChunk(ChunkPos);
        }

        /// <summary>
        /// 设置实体数据
        /// </summary>
        /// <param name="entityData"></param>
        public void SetEntityData(EntityData entityData)
        {
            if (entityData.EntityType != GetType().FullName)
                Debug.LogError("Wrong entity data set to entity");
            _entityData = entityData;

            LoadFromEntityData();
        }

        /// <summary>
        /// 获取实体数据
        /// </summary>
        /// <returns></returns>
        public EntityData GetEntityData()
        {
            if (_entityData == null)
                _entityData = EntityData.Empty();
            SaveToEntityData();

            return _entityData;
        }

        void Start()
        {
            Owner = transform.parent?.GetComponent<EntityContainer>();
        }

        void Update()
        {
            // 初始化时不刷新实体
            if (GameManager.IsInitializing || !Owner.IsLoaded)
                return;

            // 玩家移动或者所在Chunk未加载时刷新所有者信息
            if (transform.hasChanged || !_canMove)
            {
                transform.hasChanged = false;

                // 刷新实体所有者
                UpdateOwner();

                // 所在Chunk无效时不刷新实体
                var chunk = GetChunk();
                _canMove = chunk != null && chunk.IsPhysicsReady.GetValueOrDefault();
            }

            // 刷新实体移动
            if (_canMove)
                UpdateMovement();
        }

        /// <summary>
        /// 刷新所有者
        /// </summary>
        private void UpdateOwner()
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

        /// <summary>
        /// 实例化实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public static T Create<T>(EntityContainer container, string entityName = null) where T : Entity
        {
            if (container.IsGlobalContainer)
                Debug.LogError("Can't create entity in a global entity container");

            return (T) Create(container, typeof(T), entityName);
        }

        /// <summary>
        /// 实例化实体
        /// </summary>
        /// <param name="container"></param>
        /// <param name="type"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
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
