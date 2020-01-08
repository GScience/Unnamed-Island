using Island.Game.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Proxy.Items
{
    public abstract class ItemBase : IItem
    {
        private Sprite _dropSprite;
        private Sprite _itemSprite;

        public abstract string Name { get; }

        protected abstract string GetDropSpriteName();
        protected abstract string GetItemSpriteName();

        public Sprite GetDropSprite()
        {
            return _dropSprite;
        }

        public Sprite GetItemSprite()
        {
            return _itemSprite;
        }

        public virtual void Init()
        {
            _dropSprite = GameManager.ItemSpriteDatabase.Get(GetDropSpriteName());
            _itemSprite = GameManager.ItemSpriteDatabase.Get(GetItemSpriteName());
        }

        public abstract int GetMaxStackCount();
    }
}
