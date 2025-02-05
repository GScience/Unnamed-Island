﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Proxy.Blocks;
using Island.Game.EntityBehaviour;
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
            air = GameManager.ProxyManager.Get<IBlock>("island.block:air");
            dirt = GameManager.ProxyManager.Get<IBlock>("island.block:dirt");
            grass = GameManager.ProxyManager.Get<IBlock>("island.block:grass");
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
                            blocks[x, y, z].blockProxy = air;
                        else if (y == height)
                            blocks[x, y, z].blockProxy = grass;
                        else
                            blocks[x, y, z].blockProxy = dirt;
                    }
                }
        }
        public override void GenChunkEntity(ChunkPos chunkPos, ref List<DataTag> entityData)
        {
            var count = _random.Next() % 50 + 1;

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

                var type = _random.Next(0, 3);

                if (type == 0)
                {
                    var envElement = new DataTag(
                        new Dictionary<string, object>
                        {
                        {
                            "name", "EnvElement"
                        },
                        {
                            "type", "island.entity:env_element"
                        },
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
                else if (type == 1)
                {
                    var envElement = new DataTag(
                        new Dictionary<string, object>
                        {
                        {
                            "name", "EnvElement"
                        },
                        {
                            "type", "island.entity:env_element"
                        },
                        {
                            "position", pos
                        },
                        {
                            "envElement", "island.env_element:grass"
                        }
                        }
                        );
                    entityData.Add(envElement);
                }
                else if (type == 2)
                {
                    var dropStone = new DataTag(
                       new Dictionary<string, object>
                       {
                       {
                            "name", "DropStone"
                       },
                       {
                            "type", "island.entity:drop_item"
                       },
                       {
                            "position", pos + Vector3.up * 0.5f
                       },
                       {
                            "item", "island.item:stone"
                       },
                       {
                            "itemCount", 1
                       }
                       }
                       );
                    entityData.Add(dropStone);
                }
            }

            if (chunkPos.x == 1 && chunkPos.z == 1)
            {
                var height2 = GetHeight(
                        chunkPos.x * worldInfo.chunkSize.x,
                        chunkPos.z * worldInfo.chunkSize.z,
                        worldInfo.chunkSize.y / 3 * 2);

                var pos2 = new Vector3(
                        chunkPos.x * worldInfo.chunkSize.x + 0.5f * worldInfo.blockSize.x,
                        height2 * worldInfo.blockSize.y,
                        chunkPos.z * worldInfo.chunkSize.z + 0.5f * worldInfo.blockSize.z);

                var chest = new DataTag(
                    new Dictionary<string, object>
                    {
                        {
                            "name", "Chest"
                        },
                        {
                            "type", "island.entity:chest"
                        },
                        {
                            "position", pos2 - Vector3.left
                        }
                    }
                    );

                entityData.Add(chest);

                var craftTable = new DataTag(
                    new Dictionary<string, object>
                    {
                        {
                            "name", "CraftTable"
                        },
                        {
                            "position", pos2 + Vector3.left
                        },
                        {
                            "type", "island.entity:craft_table"
                        }
                    }
                    );
                entityData.Add(craftTable);
            }
        }

        public override void GenGlobalEntity(ref List<DataTag> globalEntityList)
        {
            var playerData = new DataTag(
                new Dictionary<string, object>
                {
                    {
                        "name", "Player"
                    },
                    {
                        "type", "island.entity:player"
                    },
                    {
                        "position", new Vector3(16, 100, 16)
                    }
                });

            var cameraData = new DataTag(
                new Dictionary<string, object>
                {
                    {
                        "name", "Camera"
                    },
                    {
                        "type", "island.entity:game_camera"
                    }
                });

            globalEntityList.Add(playerData);
            globalEntityList.Add(cameraData);
        }

        public int GetHeight(int x, int z, float maxHeight, float scale = 1 / 200.0f)
        {
            return (int)(Mathf.PerlinNoise(x * scale, z * scale) * maxHeight);
        }
    }
}
