using island.Game.Extension.World;
using Island.Game.System;
using Island.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Extension.World
{
    /// <summary>
    /// 世界扩展，生成常用实体等
    /// </summary>
    public static class WorldExtension
    {
        public static bool CreateDropItem(this WorldManager worldManager, string itemName, int itemCount, Vector3 pos)
        {
            var chunkPos = new ChunkPos(pos);

            var chunk = worldManager.GetChunk(chunkPos);

            if (chunk == null)
                return false;

            chunk.CreateDropItem(itemName, itemCount, pos);

            return true;
        }
    }
}
