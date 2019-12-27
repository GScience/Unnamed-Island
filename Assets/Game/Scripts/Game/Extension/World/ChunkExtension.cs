using Island.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace island.Game.Extension.World
{
    /// <summary>
    /// 区块扩展，生成常用实体等
    /// </summary>
    public static class ChunkExtension
    {
        public static void CreateDropItem(this Chunk chunk, string itemName, int itemCount, Vector3 pos)
        {
            chunk.CreateEntity(new DataTag(
                new Dictionary<string, object>
                {
                                {"type", "island.entity:drop_item" },
                                {"name", "dropItem" },
                                {"position", pos + Vector3.up },
                                {"velocity", Vector3.up },
                                {"item", itemName },
                                {"itemCount", itemCount }
                }));
        }
    }
}
