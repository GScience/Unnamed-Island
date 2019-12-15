﻿using System;
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
    [RequireComponent(typeof(CharacterController))]
    public abstract class Entity : MonoBehaviour
    {
        public ChunkPos ChunkPos => new ChunkPos(
            Mathf.FloorToInt(transform.position.x / 
                (GameManager.WorldManager.ChunkSize.x * GameManager.WorldManager.BlockSize.x)), 
            Mathf.FloorToInt(transform.position.z / 
                (GameManager.WorldManager.ChunkSize.z * GameManager.WorldManager.BlockSize.z)));

        public EntityContainer Owner => transform.parent?.GetComponent<EntityContainer>();
        public EntityData entityData = new EntityData();

        public ChunkContainer GetChunk(ChunkPos chunkPos)
        {
            return GameManager.WorldManager.GetChunk(chunkPos);
        }

        void Update()
        {
            // 初始化时不刷新实体
            if (GameManager.IsInitializing)
                return;

            // 所在Chunk无效时不刷新实体
            var chunk = GetChunk(ChunkPos);
            if (chunk == null || chunk.IsPhysicsReady != true)
                return;

            // 刷新实体所有者
            UpdateOwner();

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
                    chunk.EntityContainer.Add(this);
                }
            }
        }

        protected abstract void UpdateMovement();

        public virtual void SaveToEntityData()
        {
            entityData.Set("position", transform.position);
        }

        public virtual void Refresh()
        {
            transform.position = entityData.TryGetVector3("position", transform.position);
        }

        public static T Create<T>(EntityContainer container, string entityName = null) where T : Entity
        {
            if (container.IsGlobalContainer)
                Debug.LogError("Can't create entity in a global entity container");

            return (T) Create(container, typeof(T));
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
