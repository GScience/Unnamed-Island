using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Island.Game.World
{
    /// <summary>
    /// 世界加载信息
    /// 游戏开始时场景中必须包含，用来决定加载哪个世界
    /// </summary>
    public class WorldInfo : MonoBehaviour
    {
        public const int WorldVersion = 1;

        public string worldPath;
        public string worldName;
        public string worldGeneratorType;

        public ChunkPos worldSize;
        public Vector3 blockSize;
        public Vector3Int chunkSize;

        public void Save(BinaryWriter writer)
        {
            writer.Write(WorldVersion);

            writer.Write(worldName);
            writer.Write(worldGeneratorType);

            writer.Write(worldSize.x);
            writer.Write(worldSize.z);

            writer.Write(blockSize.x);
            writer.Write(blockSize.y);
            writer.Write(blockSize.z);

            writer.Write(chunkSize.x);
            writer.Write(chunkSize.y);
            writer.Write(chunkSize.z);
        }

        public void Load(BinaryReader reader)
        {
            var version = reader.ReadInt32();

            if (version != WorldVersion)
            {
                Debug.LogError("Unsupport to update");
                // UpdateWorld(version, WorldGenerator.GeneratorVersion);
            }

            worldName = reader.ReadString();
            worldGeneratorType = reader.ReadString();

            var worldSizeX = reader.ReadInt32();
            var worldSizeZ = reader.ReadInt32();

            var blockSizeX = reader.ReadSingle();
            var blockSizeY = reader.ReadSingle();
            var blockSizeZ = reader.ReadSingle();

            var chunkSizeX = reader.ReadInt32();
            var chunkSizeY = reader.ReadInt32();
            var chunkSizeZ = reader.ReadInt32();

            blockSize = new Vector3(blockSizeX, blockSizeY, blockSizeZ);
            chunkSize = new Vector3Int(chunkSizeX, chunkSizeY, chunkSizeZ);
            worldSize = new ChunkPos(worldSizeX, worldSizeZ);
        }
    }
}