using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.World;
using Island.Game.Proxy.Items;
using Island.Game.System;
using UnityEngine;

namespace Island.Game.EntityBehaviour
{
    public class InventoryBehaviour : EntityBehaviour
    {
        private Item[] _items = new Item[0];

        public int inventorySize = 1;

        private void Awake()
        {
            _items = new Item[inventorySize];

            for (var i = 0; i < _items.Length; ++i)
                _items[i] = new Item();
        }

        protected override void EntityLoad(DataTag dataTag)
        {
            if (inventorySize < 1)
                Debug.LogError("Inventory size should not be less than 1");

            var itemListCount = dataTag.Get<int>("itemCount");

            for (var i = 0; i < Mathf.Min(itemListCount, inventorySize); ++i)
            {
                var itemName = dataTag.Get<string>($"item{i}.name");
                var itemCount = dataTag.Get<int>($"item{i}.count");

                if (string.IsNullOrEmpty(itemName))
                    continue;

                _items[i].itemProxy = GameManager.ProxyManager.Get<IItem>(itemName);
                _items[i].count = itemCount;
            }
        }

        protected override void EntitySave(DataTag dataTag)
        {
            dataTag.Set("itemCount", _items.Length);

            for (var i = 0; i < _items.Length; ++i)
            {
                dataTag.Set($"item{i}.name", _items[i]?.itemProxy?.Name ?? "");
                dataTag.Set($"item{i}.count", _items[i]?.count ?? 0);
            }
        }

        /// <summary>
        /// 向物品栏中添加物品
        /// </summary>
        /// <param name="item">无法添加的物品</param>
        /// <returns></returns>
        public void AddItem(ref Item addedItem)
        {
            if (addedItem == null)
                Debug.LogError("Can't add a null item to inventory");

            var firstEmptyPos = int.MaxValue;
            var maxCount = addedItem.itemProxy.GetMaxStackCount();

            for (var i = 0; i < _items.Length; ++i)
            {
                if (_items[i].itemProxy == null)
                {
                    firstEmptyPos = Mathf.Min(firstEmptyPos, i);
                    continue;
                }

                if (_items[i].itemProxy != addedItem.itemProxy)
                    continue;

                _items[i].count += addedItem.count;

                if (_items[i].count <= maxCount)
                    addedItem.count = 0;
                else
                {
                    addedItem.count = _items[i].count - maxCount;
                    _items[i].count = maxCount;
                }

                if (addedItem.count == 0)
                    break;
            }

            if (addedItem.count == 0 || firstEmptyPos == int.MaxValue)
                return;

            _items[firstEmptyPos].itemProxy = addedItem.itemProxy;
            _items[firstEmptyPos].count = addedItem.count;

            if (_items[firstEmptyPos].count <= maxCount)
                addedItem.count = 0;
            else
            {
                addedItem.count = _items[firstEmptyPos].count - maxCount;
                _items[firstEmptyPos].count = maxCount;
            }
        }

        protected override void Init()
        {
            
        }

        public Item GetItem(int slotId)
        {
            if (slotId >= _items.Length)
                return null;
            return _items[slotId];
        }
    }
}
