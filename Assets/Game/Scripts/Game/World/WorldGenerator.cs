using Island.Game.System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Island.Game.World
{
    /// <summary>
    /// 世界生成器接口
    /// 用于以后的多世界（或者其他类型的世界的解锁）
    /// </summary>
    public abstract class WorldGenerator : MonoBehaviour
    {
        public abstract void GenChunk(ChunkPos chunkPos, ref Block[,,] blocks);
        protected abstract void Init();

        protected string outDir;

        public WorldInfo worldInfo;

        private void Awake()
        {
            Init();
            worldInfo = GameManager.WorldManager?.worldInfo;
        }
        public void Generate(string worldName, ChunkPos worldSize, Vector3 blockSize, Vector3Int chunkSize)
        {
            worldInfo = new GameObject().AddComponent<WorldInfo>();

            worldInfo.worldName = worldName;
            worldInfo.worldGeneratorType = GetType().FullName;
            worldInfo.chunkSize = chunkSize;
            worldInfo.worldSize = worldSize;
            worldInfo.blockSize = blockSize;

            // 获取目录位置
            outDir = Application.persistentDataPath + "/world/" + worldName;

            while (Directory.Exists(outDir))
                outDir += "-";
            outDir += "/";

            Directory.CreateDirectory(outDir);

            worldInfo.worldPath = outDir;

            // 创建世界基本信息
            GenerateWorldInfo();
        }

        private void GenerateWorldInfo()
        {
            using (var worldInfoStream = File.Create(outDir + "world.dat"))
                using (var worldInfoWriter = new BinaryWriter(worldInfoStream))
                    worldInfo.Save(worldInfoWriter);
        }
    }
}
