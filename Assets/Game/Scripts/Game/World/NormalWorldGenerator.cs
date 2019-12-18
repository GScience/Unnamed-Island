﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Data.Blocks;
using Island.Game.Entitys;
using Island.Game.System;
using UnityEngine;
using Random = System.Random;

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

        private Random _random = new Random();

        protected override void Init()
        {
            air = GameManager.DataManager.Get<IBlock>("island.block:air");
            dirt = GameManager.DataManager.Get<IBlock>("island.block:dirt");
            grass = GameManager.DataManager.Get<IBlock>("island.block:grass");
        }

        public override void GenChunk(ChunkPos chunkPos, ref Block[,,] blocks)
        {
            // 生成地形
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
        public override void GenChunkEntity(ChunkPos chunkPos, ref List<EntityData> entityData)
        {
            var count = _random.Next() % 50;

            for (var i = 0; i < count; ++i)
            {
                var x = _random.Next() % 16;
                var z = _random.Next() % 16;

                var height = GetHeight(
                    chunkPos.x * worldInfo.chunkSize.x + x,
                    chunkPos.z * worldInfo.chunkSize.z + z,
                    worldInfo.chunkSize.y / 3 * 2);

                var pos = new Vector3(
                    chunkPos.x * worldInfo.chunkSize.x + (x + 0.5f) * worldInfo.blockSize.x,
                    height * worldInfo.blockSize.y,
                    chunkPos.z * worldInfo.chunkSize.z + (z + 0.5f) * worldInfo.blockSize.z);

                var envElement = new EntityData(
                    "EnvElement",
                    typeof(EnvElement),
                    new Dictionary<string, object>
                    {
                            {
                                "position", pos
                            },
                            {
                                "envElement", "island.env_element:withered_grass"
                            }
                    }
                    );

                entityData.Add(envElement);
            }
        }

        public override void GenGlobalEntity(ref List<EntityData> globalEntityList)
        {
            var playerData = new EntityData("Player", typeof(Player));
            playerData.Set("position", new Vector3(16, 100, 16));

            globalEntityList.Add(playerData);
        }

        public int GetHeight(int x, int z, float maxHeight, float scale = 1 / 200.0f)
        {
            return (int)(Mathf.PerlinNoise(x * scale, z * scale) * maxHeight);
        }
    }
}
