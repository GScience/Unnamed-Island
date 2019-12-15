using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.World
{
    /// <summary>
    /// 区块坐标
    /// </summary>
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

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(ChunkPos chunkPos1, ChunkPos chunkPos2)
        {
            return chunkPos1.x == chunkPos2.x && chunkPos1.z == chunkPos2.z;
        }

        public static bool operator !=(ChunkPos chunkPos1, ChunkPos chunkPos2)
        {
            return !(chunkPos1 == chunkPos2);
        }
    }
}
