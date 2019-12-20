using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Render;
using Island.Game.System;

namespace Island.Game.Proxy.Blocks
{
    public enum Face
    {
        Up, Down, Left, Right,Forward, Back
    }
    public interface IBlock : IProxy
    {
        bool IsAlpha { get; }
        BlockTexture GetFaceTexture(Face face);
    }
}
