using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Render;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Island.Game.System
{
    /// <summary>
    /// 方块纹理管理器，负责记录方块的纹理信息
    /// </summary>
    public class BlockTextureManager : MonoBehaviour
    {
        /// <summary>
        /// 纹理列表
        /// </summary>
        public List<Texture2D> textureList;

        /// <summary>
        /// 纹理索引
        /// </summary>
        private readonly Dictionary<string, BlockTexture> _textureDict = new Dictionary<string, BlockTexture>();
        public Texture2D BlocksTexture { get; private set; }
        public Material BlocksMaterial { get; private set; }

        public Shader shader;

        void Awake()
        {
            BlocksTexture = new Texture2D(512, 512)
            {
                filterMode = FilterMode.Point
            };

            BlocksMaterial = new Material(shader)
            {
                mainTexture = BlocksTexture
            };

            var x = 0;
            var y = 0;

            var maxWidth = 0;

            foreach (var texture in textureList)
            {
                // 超过水平范围
                if (x + texture.width > BlocksTexture.width)
                {
                    x += maxWidth;
                    y = 0;
                }

                // 超过垂直范围
                if (x + texture.width > BlocksTexture.width)
                {
                    Debug.LogError("Too much to the block texture manager");
                    break;
                }

                //画到纹理上
                if (!texture.isReadable)
                    Debug.LogError("Block texture should be readable");

                BlocksTexture.SetPixels(x, y, texture.width, texture.height, texture.GetPixels());

                //最大宽度
                maxWidth = Mathf.Max(maxWidth, texture.width);

                //储存纹理信息
                _textureDict[texture.name.ToLower()] = new BlockTexture
                {
                    left = (float) x / BlocksTexture.width,
                    top = (float) y / BlocksTexture.height,
                    right = (float) (x + texture.width) / BlocksTexture.width,
                    bottom = (float) (y + texture.height) / BlocksTexture.height
                };

                //移动到下一个位置
                y += texture.height;
            }

            BlocksTexture.Apply();
        }

        public BlockTexture Get(string textureName)
        {
            if (string.IsNullOrEmpty(textureName))
                return BlockTexture.empty;

            if (!_textureDict.TryGetValue(textureName.ToLower(), out var texture))
                Debug.LogError("Failed to find block texture " + textureName);

            return texture;
        }
    }
}
