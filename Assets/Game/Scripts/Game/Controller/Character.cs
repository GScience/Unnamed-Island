using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.System;
using Game.World;
using UnityEditor;
using UnityEngine;

namespace Game.Controller
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class Character : MonoBehaviour
    {
        public ChunkPos ChunkPos => new ChunkPos(
            Mathf.FloorToInt(transform.position.x / 16), 
            Mathf.FloorToInt(transform.position.z / 16));

        public ChunkContainer GetChunk(ChunkPos chunkPos)
        {
            return GameManager.WorldManager.GetChunk(chunkPos);
        }

        void Update()
        {
            var chunk = GetChunk(ChunkPos);
            if (chunk == null || chunk.PhysicsReady != true)
                return;

            UpdateMovement();
        }

        protected abstract void UpdateMovement();

#if UNITY_EDITOR
        [CustomEditor(typeof(Character))]
        class CharactorEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                var character = (Character) serializedObject.targetObject;

                GUILayout.Label("In Chunk: " + character.ChunkPos.x + character.ChunkPos.z);
                base.OnInspectorGUI();
            }
        }
#endif
    }
}
