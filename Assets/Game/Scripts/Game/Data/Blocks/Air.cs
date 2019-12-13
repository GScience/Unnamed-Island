using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Render;
using Island.Game.System;

namespace Island.Game.Data.Blocks
{
    public class Air : IBlock
    {
        public string Name => "island.block:air";
        public void Load()
        {
        }

        public bool IsAlpha => true;

        public BlockTexture GetFaceTexture(Face face)
        {
            return BlockTexture.empty;
        }
    }
}
