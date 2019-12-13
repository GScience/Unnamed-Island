using Island.Game.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        }

        public void LoadChunk(ChunkPos chunkPos, ref Block[,,] blocks)
        {
            var chunkFilePath = _dir + "c." + chunkPos.x + "." + chunkPos.z + ".dat";
            var chunkSize = GameManager.WorldManager.chunkSize;

            // Chunk不存在
            var isChunkExists = File.Exists(chunkFilePath);
            if (!isChunkExists)
            {
                for (var x = 0; x < chunkSize.x; ++x)
                    for (var y = 0; y < chunkSize.y; ++y)
                        for (var z = 0; z < chunkSize.z; ++z)
                            blocks[x, y, z].ReadFrom(null, null);

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
        }

        private void LoadWorldInfo()
        {
            using (var worldInfo = File.OpenRead(_dir + "world.dat"))
            {
                using (var worldInfoReader = new BinaryReader(worldInfo))
                {
                    var version = worldInfoReader.ReadInt32();

                    if (version != WorldGenerator.GeneratorVersion)
                        UpdateWorld(version, WorldGenerator.GeneratorVersion);

                    var worldName = worldInfoReader.ReadString();

                    var worldSizeX = worldInfoReader.ReadInt32();
                    var worldSizeZ = worldInfoReader.ReadInt32();

                    var blockSizeX = worldInfoReader.ReadSingle();
                    var blockSizeY = worldInfoReader.ReadSingle();
                    var blockSizeZ = worldInfoReader.ReadSingle();

                    var chunkSizeX = worldInfoReader.ReadInt32();
                    var chunkSizeY = worldInfoReader.ReadInt32();
                    var chunkSizeZ = worldInfoReader.ReadInt32();

                    GameManager.WorldManager.blockSize = new Vector3(blockSizeX, blockSizeY, blockSizeZ);
                    GameManager.WorldManager.chunkSize = new Vector3Int(chunkSizeX, chunkSizeY, chunkSizeZ);
                }
            }
        }

        private void UpdateWorld(int from, int to)
        {
            
        }
    }
}
