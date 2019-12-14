using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Data.Blocks;
using Island.Game.System;
using UnityEngine;

namespace Island.Game.World
{
    /// <summary>
    /// 世界生成器
    /// </summary>
    public class NormalWorldGenerator : WorldGenerator
    {
        public IBlock air;
        public IBlock dirt;
        public IBlock grass;

        protected override void Init()
        {
            air = GameManager.DataManager.Get<IBlock>("island.block:air");
            dirt = GameManager.DataManager.Get<IBlock>("island.block:dirt");
            grass = GameManager.DataManager.Get<IBlock>("island.block:grass");
        }

        public override void GenChunk(ChunkPos chunkPos, ref Block[,,] blocks)
        {
            for (var x = 0; x < worldInfo.chunkSize.x; ++x)
                for (var z = 0; z < worldInfo.chunkSize.z; ++z)
                {
                    var height = GetHeight(
                        x + chunkPos.x * worldInfo.chunkSize.x,
                        z + chunkPos.z * worldInfo.chunkSize.z,
                        worldInfo.chunkSize.y / 3 * 2);

                    for (var y = 0; y < worldInfo.chunkSize.y; ++y)
                    {
                        if (y > height)
                            blocks[x, y, z].blockData = air;
                        else if (y == height)
                            blocks[x, y, z].blockData = grass;
                        else
                            blocks[x, y, z].blockData = dirt;
                    }
                }
        }

        public int GetHeight(int x, int z, float maxHeight, float scale = 1 / 50.0f)
        {
            return (int)(Mathf.PerlinNoise(x * scale, z * scale) * maxHeight);
        }
    }
}
