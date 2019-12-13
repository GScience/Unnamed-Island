using Island.Game.System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public const int GeneratorVersion = 1;

        protected abstract void GenChunk(ChunkPos chunkPos, ref Block[,,] blocks);
        protected abstract void Init();

        protected string worldName;
        protected ChunkPos worldSize;
        protected Vector3 blockSize;
        protected Vector3Int chunkSize;

        protected string outDir;

        public UnityAction<string> onStageChange;
        public UnityAction onLoaded;

        protected DataManager dataManager;

        private void Awake()
        {
            dataManager = GameManager.DataManager;
            if (dataManager == null)
            {
                var dataManagerObj = new GameObject();
                dataManager = dataManagerObj.AddComponent<DataManager>();
            }

            Init();
        }
        public void Generate(string worldName, ChunkPos worldSize, Vector3 blockSize, Vector3Int chunkSize)
        {
            this.worldName = worldName;
            this.chunkSize = chunkSize;
            this.worldSize = worldSize;
            this.blockSize = blockSize;

            // 获取目录位置
            outDir = Application.persistentDataPath + "/world/" + worldName;

            while (Directory.Exists(outDir))
                outDir += "-";
            outDir += "/";

            Directory.CreateDirectory(outDir);

            // 创建世界基本信息
            onStageChange?.Invoke("正在理解世界");
            GenerateWorldInfo();

            // 创建Chunk数据
            onStageChange?.Invoke("火山正在喷发");
            StartCoroutine(GenerateChunks(() => onLoaded?.Invoke()));
        }

        IEnumerator GenerateChunks(Action onFinish = null)
        {
            var blocks = new Block[chunkSize.x, chunkSize.y, chunkSize.z];

            var loadedChunk = 0;
            var totalChunk = worldSize.x * worldSize.z;

            var lastStartTime = DateTime.Now;

            for (var x = 0; x < worldSize.x; ++x)
            {
                for (var z = 0; z < worldSize.z; ++z)
                {
                    var chunkPos = new ChunkPos(x, z);
                    GenChunk(chunkPos, ref blocks);
                    SaveChunk(chunkPos, blocks);

                    ++loadedChunk;

                    onStageChange?.Invoke($"火山正在喷发 {loadedChunk}/{totalChunk}");

                    var currentTime = DateTime.Now;

                    if ((currentTime - lastStartTime).TotalMilliseconds > 100)
                    {
                        yield return 1;
                        lastStartTime = DateTime.Now;
                    }
                }
            }

            onFinish?.Invoke();
        }

        private void SaveChunk(ChunkPos chunkPos, Block[,,] blocks)
        {
            var chunkStream = File.Create(outDir + "c." + chunkPos.x + "." + chunkPos.z + ".dat");
            var writer = new BinaryWriter(chunkStream);

            // 创建索引表
            List<string> blockIndex = new List<string>();

            for (var x = 0; x < chunkSize.x; ++x)
                for (var y = 0; y < chunkSize.y; ++y)
                    for (var z = 0; z < chunkSize.z; ++z)
                    {
                        var blockData = blocks[x, y, z].blockData;
                        var blockName = blockData == null ? "island.block:air" : blockData.Name;
                        if (!blockIndex.Contains(blockName))
                            blockIndex.Add(blockName);
                    }

            // 写入索引表
            writer.Write(blockIndex.Count);

            foreach (var str in blockIndex)
                writer.Write(str);

            // 写入Block信息
            for (var x = 0; x < chunkSize.x; ++x)
                for (var y = 0; y < chunkSize.y; ++y)
                    for (var z = 0; z < chunkSize.z; ++z)
                        blocks[x, y, z].WriteTo(writer, blockIndex);

            chunkStream.Close();
        }

        private void GenerateWorldInfo()
        {
            using (var worldInfo = File.Create(outDir + "world.dat"))
            {
                using (var worldInfoWriter = new BinaryWriter(worldInfo))
                {
                    worldInfoWriter.Write(GeneratorVersion);

                    worldInfoWriter.Write(worldName);

                    worldInfoWriter.Write(worldSize.x);
                    worldInfoWriter.Write(worldSize.z);

                    worldInfoWriter.Write(blockSize.x);
                    worldInfoWriter.Write(blockSize.y);
                    worldInfoWriter.Write(blockSize.z);

                    worldInfoWriter.Write(chunkSize.x);
                    worldInfoWriter.Write(chunkSize.y);
                    worldInfoWriter.Write(chunkSize.z);
                }
            }
        }
    }
}
