using Island.Game.Render;
using Island.Game.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Data.Blocks
{
    class Dirt : IBlock
    {
        private BlockTexture _dirtAround = BlockTexture.empty;
        private BlockTexture _dirtTop = BlockTexture.empty;

        public string Name => "island.block:dirt";
        public void Load()
        {
            _dirtAround = GameManager.BlockTextureManager.Get("dirtAround");
            _dirtTop = GameManager.BlockTextureManager.Get("dirtTop");
        }

        public bool IsAlpha => false;

        public BlockTexture GetFaceTexture(Face face)
        {
            switch (face)
            {
                case Face.Up:
                case Face.Down:
                    return _dirtTop;
                default:
                    return _dirtAround;
            }
        }
    }
}
