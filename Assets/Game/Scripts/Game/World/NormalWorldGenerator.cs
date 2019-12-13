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
            air = new Air();
            dirt = new Dirt();
            grass = new Grass();
        }

        protected override void GenChunk(ChunkPos chunkPos, ref Block[,,] blocks)
        {
            for (var x = 0; x < chunkSize.x; ++x)
                for (var z = 0; z < chunkSize.z; ++z)
                {
                    var height = GetHeight(
                        x + chunkPos.x * chunkSize.x,
                        z + chunkPos.z * chunkSize.z,
                        chunkSize.y / 3 * 2);

                    for (var y = 0; y < chunkSize.y; ++y)
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
