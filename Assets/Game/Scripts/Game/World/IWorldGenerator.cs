using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.World
{
    public interface IWorldGenerator
    {
        void GenChunk(ChunkPos chunkPos, ref Block[,,] blocks);
    }
}
