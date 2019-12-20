﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Proxy.Entity;
using Island.Game.System;
using Island.Game.World;
using UnityEditor;
using UnityEngine;

namespace Island.Game.World
{
    /// <summary>
    /// 实体抽象类
    /// 所有实体需要继承此类
    /// </summary>
    public class Entity : MonoBehaviour
    {
        /// <summary>
        /// 所有Entity所在层
        /// </summary>
        public const int Layer = 8;

        /// <summary>
        /// 实体是否被选择
        /// </summary>
        public bool IsSelected
        {
            set
            {
                SendMessage("OnEntitySelectChange", value, SendMessageOptions.DontRequireReceiver);
            }
        }

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

        /// <summary>
        /// 实体碰撞箱
        /// </summary>
        private SphereCollider _collider;

        /// <summary>
        /// 是否有刷新事件
        /// </summary>
        public bool HasUpdation { get; set; }

        /// <summary>
        /// 实体代理
        /// </summary>
        private IEntity _entityProxy;

        /// <summary>
        /// 实体行为
        /// </summary>
        private MonoBehaviour _entityBehaviour;

        /// <summary>
        /// 实体数据，用于序列化和反序列化
        /// </summary>
        private DataTag _entityDataTag = DataTag.Empty();

        /// <summary>
        /// 是否可以移动
        /// </summary>
        private bool _canMove;

        /// <summary>
        /// 获取实体所在Chunk
        /// </summary>
        /// <param name="chunkPos"></param>
        /// <returns></returns>
        public Chunk GetChunk()
        {
            return GameManager.WorldManager.GetChunk(ChunkPos);
        }

        /// <summary>
        /// 设置实体数据
        /// </summary>
        /// <param name="entityData"></param>
        public void SetEntityData(DataTag entityData)
        {
            // 设置代理
            var entityProxyName = entityData.Get<string>("type");

            _entityProxy = GameManager.ProxyManager.Get<IEntity>(entityProxyName);
            var entityBehaviourType = _entityProxy.GetEntityType();

            // 尝试直接获取
            _entityBehaviour = (MonoBehaviour)gameObject.GetComponent(entityBehaviourType);

            // 创建
            if (_entityBehaviour == null)
                _entityBehaviour = (MonoBehaviour)gameObject.AddComponent(entityBehaviourType);

            // 同步标签
            _entityDataTag = entityData;

            // 加载
            LoadFromEntityData();
        }

        /// <summary>
        /// 获取实体数据
        /// </summary>
        /// <returns></returns>
        public DataTag GetEntityDataTag()
        {
            SaveToEntityDataTag();
            return _entityDataTag;
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
                _canMove = chunk != null && chunk.IsPhysicsReady;
            }

            // 刷新实体移动
            if (_canMove)
                _entityBehaviour.SendMessage("EntityUpdate");

            if (!HasUpdation)
                enabled = false;
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
            if (ChunkPos != Owner.ChunkPos)
            {
                var chunk = GameManager.WorldManager.GetChunk(ChunkPos);
                if (chunk != null)
                {
                    Owner.Remove(this);
                    Owner = null;
                    chunk.AddEntity(this);
                }

                // 更改所有权
                Owner = transform.parent?.GetComponent<EntityContainer>();
            }
        }

        private void SaveToEntityDataTag()
        {
            if (_entityBehaviour == null)
                return;

            // 代理类型
            _entityDataTag.Set("type", _entityProxy?.Name ?? "");
            // 名称
            _entityDataTag.Set("name", gameObject.name);
            // 坐标
            _entityDataTag.Set("position", transform.position);

            _entityBehaviour.SendMessage("EntitySave", _entityDataTag);
        }

        private void LoadFromEntityData()
        {
            transform.position = _entityDataTag.TryGet("position", transform.position);
            gameObject.name = _entityDataTag.Get<string>("name");

            _entityBehaviour.SendMessage("EntityLoad", _entityDataTag);
        }

        public void SetCollider(Vector3 pos, float size)
        {
            if (_collider == null)
            {
                _collider = gameObject.AddComponent<SphereCollider>();
                _collider.isTrigger = true;
                _collider.enabled = false;
                _collider.radius = size;
                _collider.center = pos;
            }
            if (size > 0)
                _collider.enabled = true;
        }

        /// <summary>
        /// 实例化实体
        /// </summary>
        /// <param name="container"></param>
        /// <param name="type"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public static Entity Create(EntityContainer container, DataTag dataTag)
        {
            var entityObj = new GameObject(dataTag.Get<string>("name"));
            entityObj.layer = Layer;

            var entity = entityObj.AddComponent<Entity>();
            container.Add(entity);
            entity.Owner = container;

            entity.SetEntityData(dataTag);

            return entity;
        }
#if UNITY_EDITOR
        [CustomEditor(typeof(Entity),editorForChildClasses:true)]
        class EntityEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                var entity = (Entity) serializedObject.targetObject;

                if (!GameManager.IsInitializing)
                {
                    GUILayout.Label("In Chunk: " + entity.ChunkPos);
                }
                base.OnInspectorGUI();
            }
        }
#endif
    }
}
