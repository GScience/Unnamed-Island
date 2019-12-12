using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Render;
using Game.System;

namespace Game.Data.Block
{
    class Dirt : IBlock
    {
        private BlockTexture _grassTop = BlockTexture.empty;
        private BlockTexture _grass = BlockTexture.empty;
        private BlockTexture _dirt = BlockTexture.empty;

        public string Name => "island.block:dirt";
        public void Load()
        {
            _grassTop = GameManager.BlockTextureManager.Get("grasstop");
            _grass = GameManager.BlockTextureManager.Get("grass");
            _dirt = GameManager.BlockTextureManager.Get("dirt");
        }

        public bool IsAlpha => false;

        public BlockTexture GetFaceTexture(Face face)
        {
            switch (face)
            {
                case Face.Up:
                    return _grassTop;
                case Face.Down:
                    return _dirt;
                default:
                    return _grass;
            }
        }
    }
}
