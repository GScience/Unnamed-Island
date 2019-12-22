using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.System
{
    public class SpriteDatabase : MonoBehaviour
    {
        public List<Sprite> spriteList;

        private readonly Dictionary<string, Sprite> _spriteDict = new Dictionary<string, Sprite>();

        void Awake()
        {
            foreach (var sprite in spriteList)
                _spriteDict[sprite.name.ToLower()] = sprite;
        }

        public Sprite Get(string spriteName)
        {
            if (_spriteDict.TryGetValue(spriteName.ToLower(), out var result))
                return result;

            Debug.LogError("Sprite " + spriteName + " not found in " + name);
            return null;
        }
    }
}
