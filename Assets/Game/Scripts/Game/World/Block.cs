using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Proxy.Blocks;
using Island.Game.System;

namespace Island.Game.World
{
    /// <summary>
    /// 方块
    /// </summary>
    public struct Block
    {
        /// <summary>
        /// 方块代理
        /// </summary>
        public IBlock blockProxy;

        public void WriteTo(BinaryWriter writer, List<string> blockIndex)
        {
            var blockName = blockProxy == null ? "island.block:air" : blockProxy.Name;
            var index = (short) blockIndex.FindIndex((string s) => s == blockName);

            writer.Write(index);
        }

        public void ReadFrom(BinaryReader reader, List<string> blockIndex)
        {
            if (reader == null)
                blockProxy = GameManager.ProxyManager.Get<IBlock>("island.block:air");
            else
            {
                var index = reader.ReadInt16();
                var blockName = blockIndex[index];
                blockProxy = GameManager.ProxyManager.Get<IBlock>(blockName);
            }
        }
    }
}
