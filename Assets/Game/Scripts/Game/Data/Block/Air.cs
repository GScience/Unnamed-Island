using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Render;
using Game.System;

namespace Game.Data.Block
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
