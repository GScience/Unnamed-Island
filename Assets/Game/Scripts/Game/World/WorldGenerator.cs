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
        private enum GeneratingStage : int
        {
            LoadingWorldInfo = 0,
            LoadingChunk = 1,
            LoadingEnvElement = 2
        }

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

        private int _loadedChunk;
        private int _totalChunk;
        private bool _isLoading;
        private GeneratingStage _stage;

        private CancellationTokenSource _cts;

        private void Awake()
        {
            Init();
        }
        public void Generate(string worldName, ChunkPos worldSize, Vector3 blockSize, Vector3Int chunkSize)
        {
            if (_isLoading)
                Debug.LogError("Can't generate two world at the same time");

            _isLoading = true;
            _loadedChunk = 0;
            _totalChunk = worldSize.x * worldSize.z;

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

            _stage = 0;

            // 创建世界基本信息
            onStageChange?.Invoke("正在理解世界");
            GenerateWorldInfo();

            // 创建Chunk数据
            onStageChange?.Invoke("火山正在喷发");
            GenerateChunksAsync();
        }

        void GenerateChunksAsync()
        {
            _cts = new CancellationTokenSource();

            var chunkLoadingTask = new Task(() =>
            {
                var blocks = new Block[chunkSize.x, chunkSize.y, chunkSize.z];

                for (var x = 0; x < worldSize.x; ++x)
                {
                    for (var z = 0; z < worldSize.z; ++z)
                    {
                        var chunkPos = new ChunkPos(x, z);
                        GenChunk(chunkPos, ref blocks);
                        SaveChunk(chunkPos, blocks);

                        ++_loadedChunk;
                    }
                }

                ++_stage;
            }, _cts.Token);

            chunkLoadingTask.Start();
        }

        private void GenerateEnvElement()
        {
            ++_stage;
        }

        private void OnDisable()
        {
            _cts.Cancel();
        }

        private void Update()
        {
            if (!_isLoading)
                return;

            switch (_stage)
            {
                case GeneratingStage.LoadingChunk:
                    onStageChange?.Invoke($"火山正在喷发 {_loadedChunk}/{_totalChunk}");
                    break;
                case GeneratingStage.LoadingEnvElement:
                    GenerateEnvElement();
                    break;
                default:
                    onLoaded?.Invoke();
                    _isLoading = false;
                    break;
            }
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

            writer.Close();
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
            ++_stage;
        }
    }
}
