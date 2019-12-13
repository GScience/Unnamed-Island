using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Island.Game.World
{
    /// <summary>
    /// 世界加载信息
    /// 游戏开始时场景中必须包含，用来决定加载哪个世界
    /// </summary>
    public class WorldInfo : MonoBehaviour
    {
        public string worldPath;

        public ChunkPos chunkCount;
        public Vector3 blockSize;
        public Vector3Int chunkSize;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}