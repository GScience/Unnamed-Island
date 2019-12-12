using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Render;
using Game.System;

namespace Game.Data.Block
{
    public enum Face
    {
        Up, Down, Left, Right,Forward, Back
    }
    public interface IBlock : IData
    {
        bool IsAlpha { get; }
        BlockTexture GetFaceTexture(Face face);
    }
}
