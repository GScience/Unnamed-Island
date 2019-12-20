using Island.Game.GlobalEntity;
using Island.Game.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.World
{
    /// <summary>
    /// 世界加载器
    /// 用来加载区块方块，实体等
    /// </summary>
    public class WorldLoader : MonoBehaviour
    {
        private string _dir = "";

        public void LoadWorld(string worldPath)
        {
            _dir = Application.persistentDataPath + "/world/" + worldPath + "/";

            try
            {
                LoadWorldInfo();
            }
            catch
            {
                Debug.LogError("Failed to load world at " + _dir);
            }
        }

        /// <summary>
        /// 保存全局实体
        /// </summary>
        /// <param name="entitys"></param>
        private void SaveEntity(string filePath, List<DataTag> entityDataList)
        {
            var savePath = _dir + "e." + filePath + ".dat";
            var saveTmpPath = savePath + ".tmp";
            var entityStream = File.Create(saveTmpPath);
            var writer = new BinaryWriter(entityStream);

            // 保存实体
            writer.Write(entityDataList.Count);
            foreach (var entityData in entityDataList)
            {
                // 实体数据
                entityData.Save(writer);
            }

            writer.Close();
            entityStream.Close();

            // 复制文件
            File.Copy(saveTmpPath, savePath, true);

            // 删除临时文件
            File.Delete(saveTmpPath);
        }

        /// <summary>
        /// 保存实体
        /// </summary>
        /// <param name="entitys"></param>
        public void SaveEntity(ChunkPos chunkPos, List<DataTag> entityDataList)
        {
            if (!chunkPos.IsAvailable())
                return;

            if (!chunkPos.IsGlobal())
                SaveEntity(chunkPos.x + "." + chunkPos.z, entityDataList);
            else
                SaveEntity("global", entityDataList);
        }

        /// <summary>
        /// 加载实体
        /// </summary>
        /// <param name="entitys"></param>
        public void LoadEntity(ChunkPos chunkPos, ref List<DataTag> entityDataList)
        {
            var entityDataName = chunkPos.IsGlobal() ? "global" : chunkPos.x + "." + chunkPos.z;

            var loadPath = _dir + "e." + entityDataName + ".dat";

            if (!chunkPos.IsAvailable())
                return;

            // 如果未创建实体信息则生成
            if (!File.Exists(loadPath))
            {
                var worldSize = GameManager.WorldManager.worldInfo.worldSize;
                if (!chunkPos.IsGlobal())
                {
                    if (
                        !(chunkPos.x < 0 || chunkPos.x > worldSize.x || 
                        chunkPos.z < 0 || chunkPos.z > worldSize.z))
                        GameManager.WorldManager.worldGenerator.GenChunkEntity(chunkPos, ref entityDataList);
                }
                else
                    GameManager.WorldManager.worldGenerator.GenGlobalEntity(ref entityDataList);
                return;
            }

            var entityStream = File.OpenRead(loadPath);
            var reader = new BinaryReader(entityStream);

            // 加载实体
            var entityCount = reader.ReadInt32();

            for (var i = 0; i < entityCount; ++i)
            {
                var entityData = DataTag.Empty();
                entityData.Load(reader);

                entityDataList.Add(entityData);
            }

            reader.Close();
            entityStream.Close();
        }

        public void SaveBlock(ChunkPos chunkPos, Block[,,] blocks)
        {
            if (_dir == "")
            {
                Debug.LogError("Not load world");
                return;
            }

            if (!chunkPos.IsAvailable())
                return;

            var chunkSize = GameManager.WorldManager.ChunkSize;

            var savePath = _dir + "c." + chunkPos.x + "." + chunkPos.z + ".dat";
            var saveTmpPath = savePath + ".tmp";
            var chunkStream = File.Create(saveTmpPath);
            var writer = new BinaryWriter(chunkStream);

            // 创建索引表
            List<string> blockIndex = new List<string>();

            for (var x = 0; x < chunkSize.x; ++x)
                for (var y = 0; y < chunkSize.y; ++y)
                    for (var z = 0; z < chunkSize.z; ++z)
                    {
                        var blockData = blocks[x, y, z].blockProxy;
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

            // 复制文件
            File.Copy(saveTmpPath, savePath, true);

            // 删除临时文件
            File.Delete(saveTmpPath);
        }

        public void LoadBlock(ChunkPos chunkPos, ref Block[,,] blocks)
        {
            if (_dir == "")
            {
                Debug.LogError("Not load world");
                return;
            }

            if (!chunkPos.IsAvailable())
                return;

            var chunkFilePath = _dir + "c." + chunkPos.x + "." + chunkPos.z + ".dat";
            var chunkTmpFilePath = chunkFilePath + ".tmp";

            while (File.Exists(chunkTmpFilePath))
                Thread.Yield();

            var chunkSize = GameManager.WorldManager.ChunkSize;

            // Chunk不存在
            var isChunkExists = File.Exists(chunkFilePath);
            if (!isChunkExists)
            {
                var worldSize = GameManager.WorldManager.worldInfo.worldSize;
                if (chunkPos.x < 0 || chunkPos.x > worldSize.x ||
                    chunkPos.z < 0 || chunkPos.z > worldSize.z)
                {
                    for (var x = 0; x < chunkSize.x; ++x)
                        for (var y = 0; y < chunkSize.y; ++y)
                            for (var z = 0; z < chunkSize.z; ++z)
                                blocks[x, y, z].ReadFrom(null, null);
                }
                else
                {
                    GameManager.WorldManager.worldGenerator.GenChunk(chunkPos, ref blocks);
                }

                return;
            }

            var chunkStream = File.OpenRead(chunkFilePath);
            var reader = new BinaryReader(chunkStream);

            // 读取索引表
            var indexSize = reader.ReadInt32();
            var blockIndex = new List<string>(indexSize);
            for (var i = 0; i < indexSize; ++i)
                blockIndex.Add(reader.ReadString());

            for (var x = 0; x < chunkSize.x; ++x)
                for (var y = 0; y < chunkSize.y; ++y)
                    for (var z = 0; z < chunkSize.z; ++z)
                        blocks[x, y, z].ReadFrom(reader, blockIndex);

            reader.Close();
            chunkStream.Close();
        }

        private void LoadWorldInfo()
        {
            using (var worldInfoStream = File.OpenRead(_dir + "world.dat"))
            using (var worldInfoReader = new BinaryReader(worldInfoStream))
                GameManager.WorldManager.worldInfo.Load(worldInfoReader);
        }

        private void UpdateWorld(int from, int to)
        {

        }
    }
}
