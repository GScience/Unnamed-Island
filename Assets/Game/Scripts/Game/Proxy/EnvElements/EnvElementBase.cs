using Island.Game.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Proxy.EnvElements
{
    /// <summary>
    /// 环境元素数据基类
    /// 用来通过SpriteName自动优化获取Sprite
    /// </summary>
    public abstract class EnvElementBase : IEnvElement
    {
        private Sprite _sprite;

        public abstract string Name { get; }

        public Sprite GetEnvElementSprite()
        {
            return _sprite;
        }

        protected abstract string GetSpriteName();

        public void Init()
        {
            _sprite = GameManager.EnvElementSpriteDatabase.Get(GetSpriteName());
        }

        public abstract float GetColliderSize();
        public abstract Vector3 GetColliderCenter();
    }
}
