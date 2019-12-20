using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Render;
using Island.Game.System;

namespace Island.Game.Proxy.Blocks
{
    class Grass : IBlock
    {
        private BlockTexture _grassTop = BlockTexture.empty;
        private BlockTexture _grassAround = BlockTexture.empty;
        private BlockTexture _dirtTop = BlockTexture.empty;

        public string Name => "island.block:grass";
        public void Init()
        {
            _grassTop = GameManager.BlockTextureManager.Get("grassTop");
            _grassAround = GameManager.BlockTextureManager.Get("grassAround");
            _dirtTop = GameManager.BlockTextureManager.Get("dirtTop");
        }

        public bool IsAlpha => false;

        public BlockTexture GetFaceTexture(Face face)
        {
            switch (face)
            {
                case Face.Up:
                    return _grassTop;
                case Face.Down:
                    return _dirtTop;
                default:
                    return _grassAround;
            }
        }
    }
}
