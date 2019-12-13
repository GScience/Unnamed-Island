using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Data.Blocks;
using Island.Game.System;

namespace Island.Game.World
{
    /// <summary>
    /// 方块
    /// </summary>
    public struct Block
    {
        public IBlock blockData;

        public void WriteTo(BinaryWriter writer, List<string> blockIndex)
        {
            var blockName = blockData == null ? "island.block:air" : blockData.Name;
            var index = (short) blockIndex.FindIndex((string s) => s == blockName);

            writer.Write(index);
        }

        public void ReadFrom(BinaryReader reader, List<string> blockIndex)
        {
            if (reader == null)
                blockData = GameManager.DataManager.Get<IBlock>("island.block:air");
            else
            {
                var index = reader.ReadInt16();
                var blockName = blockIndex[index];
                blockData = GameManager.DataManager.Get<IBlock>(blockName);
            }
        }
    }
}
