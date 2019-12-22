using Island.Game.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Proxy.Item
{
    public abstract class ItemBase : IItem
    {
        private Sprite _fallingSprite;
        private Sprite _itemSprite;

        public abstract string Name { get; }

        protected abstract string GetFallingSpriteName();
        protected abstract string GetItemSpriteName();

        public Sprite GetFallingSprite()
        {
            return _fallingSprite;
        }

        public Sprite GetItemSprite()
        {
            return _itemSprite;
        }

        public virtual void Init()
        {
            _fallingSprite = GameManager.ItemSpriteDatabase.Get(GetFallingSpriteName());
            _itemSprite = GameManager.ItemSpriteDatabase.Get(GetItemSpriteName());
        }
    }
}
