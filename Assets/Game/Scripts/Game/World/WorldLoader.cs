﻿using Island.Game.System;
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

        public void SaveChunk(ChunkPos chunkPos, Block[,,] blocks)
        {
            if (_dir == "")
            {
                Debug.LogError("Not load world");
                return;
            }

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

            // 复制文件
            File.Copy(saveTmpPath, savePath, true);

            // 删除临时文件
            File.Delete(saveTmpPath);
        }

        public void LoadChunk(ChunkPos chunkPos, ref Block[,,] blocks)
        {
            if (_dir == "")
            {
                Debug.LogError("Not load world");
                return;
            }

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
