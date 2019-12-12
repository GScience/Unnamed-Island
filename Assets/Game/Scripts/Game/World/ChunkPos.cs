using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.World
{
    public struct ChunkPos
    {
        public int x;
        public int z;

        public ChunkPos(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public bool IsAvailable()
        {
            return x != int.MaxValue;
        }

        public static ChunkPos nonAvailable = new ChunkPos(int.MaxValue, int.MaxValue);
    }
}
