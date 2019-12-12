using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Render;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Game.System
{
    public class BlockTextureManager : MonoBehaviour
    {
        public List<Texture2D> textureList;

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

            var maxHeight = 0;

            foreach (var texture in textureList)
            {
                // 超过水平范围
                if (x + texture.width > BlocksTexture.width)
                {
                    x = 0;
                    y += maxHeight;
                }

                // 超过垂直范围
                if (y + texture.height > BlocksTexture.height)
                {
                    Debug.LogError("Too big to the block texture manager");
                    break;
                }

                //画到纹理上
                if (!texture.isReadable)
                    Debug.LogError("Block texture should be readable");

                BlocksTexture.SetPixels(x, y, texture.width, texture.height, texture.GetPixels());

                //最大高度
                maxHeight = Mathf.Max(maxHeight, texture.height);

                //储存纹理信息
                _textureDict[texture.name.ToLower()] = new BlockTexture
                {
                    left = (float) x / BlocksTexture.width,
                    top = (float) y / BlocksTexture.height,
                    right = (float) (x + texture.width) / BlocksTexture.width,
                    bottom = (float) (y + texture.height) / BlocksTexture.height
                };

                //移动到下一个位置
                x += texture.width;
            }

            BlocksTexture.Apply();
        }

        public BlockTexture Get(string textureName)
        {
            if (string.IsNullOrEmpty(textureName))
                return BlockTexture.empty;

            if (!_textureDict.TryGetValue(textureName, out var texture))
                Debug.LogError("Failed to find block texture " + textureName);

            return texture;
        }
    }
}
