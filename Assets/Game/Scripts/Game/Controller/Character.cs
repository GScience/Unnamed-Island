using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.System;
using Island.Game.World;
using UnityEditor;
using UnityEngine;

namespace Island.Game.Controller
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class Character : MonoBehaviour
    {
        public ChunkPos ChunkPos => new ChunkPos(
            Mathf.FloorToInt(transform.position.x / 
                (GameManager.WorldManager.ChunkSize.x * GameManager.WorldManager.BlockSize.x)), 
            Mathf.FloorToInt(transform.position.z / 
                (GameManager.WorldManager.ChunkSize.z * GameManager.WorldManager.BlockSize.z)));

        public ChunkContainer GetChunk(ChunkPos chunkPos)
        {
            return GameManager.WorldManager.GetChunk(chunkPos);
        }

        void Update()
        {
            if (GameManager.IsInitializing)
                return;

            var chunk = GetChunk(ChunkPos);
            if (chunk == null || chunk.IsPhysicsReady != true)
                return;

            UpdateMovement();
        }

        protected abstract void UpdateMovement();

#if UNITY_EDITOR
        [CustomEditor(typeof(Character),editorForChildClasses:true)]
        class CharactorEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                var character = (Character) serializedObject.targetObject;

                if (!GameManager.IsInitializing)
                    GUILayout.Label("In Chunk: " + character.ChunkPos.x + character.ChunkPos.z);
                base.OnInspectorGUI();
            }
        }
#endif
    }
}
