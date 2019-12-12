using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Data.Block;
using Game.System;
using UnityEngine;

namespace Game.World
{
    public class NormalWorldGenerator : IWorldGenerator
    {
        public readonly IBlock air = GameManager.DataManager.Get<IBlock>("island.block:air");
        public readonly IBlock dirt = GameManager.DataManager.Get<IBlock>("island.block:dirt");

        public readonly Vector3Int chunkSize = GameManager.WorldManager.chunkSize;

        public void GenChunk(ChunkPos chunkPos, ref Block[,,] blocks)
        {
            for (var x = 0; x < chunkSize.x; ++x)
                for (var y = 0; y < chunkSize.y; ++y)
                    for (var z = 0; z < chunkSize.z; ++z)
                    {
                        if (y > GetHeight(
                                x + chunkPos.x * chunkSize.x,
                                z + chunkPos.z * chunkSize.z,
                                chunkSize.y / 3 * 2))
                            blocks[x, y, z].blockData = air;
                        else
                            blocks[x, y, z].blockData = dirt;
                    }
        }

        public int GetHeight(int x, int z, float maxHeight, float scale = 1 / 50.0f)
        {
            return (int)(Mathf.PerlinNoise(x * scale, z * scale) * maxHeight);
        }
    }
}
