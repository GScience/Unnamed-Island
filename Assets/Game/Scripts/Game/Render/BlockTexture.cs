using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Render
{
    /// <summary>
    /// 方块纹理
    /// 通过BlockTextureManager创建
    /// </summary>
    public struct BlockTexture
    {
        public float left;
        public float top;
        public float right;
        public float bottom;

        public static BlockTexture empty = new BlockTexture
        {
            left = 0,
            top = 0,
            right = 0,
            bottom = 0
        };

        public bool IsEmpty()
        {
            return Mathf.Abs(left) < float.Epsilon &&
                   Mathf.Abs(top) < float.Epsilon &&
                   Mathf.Abs(right) < float.Epsilon &&
                   Mathf.Abs(bottom) < float.Epsilon;
        }
    }

}
