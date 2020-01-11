using Island.Game.EntityBehaviour;
using Island.Game.Proxy.Items;
using Island.Game.Proxy.Recipes;
using Island.Game.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.UI.Pannels.Inventory
{
    [RequireComponent(typeof(Pannel))]
    public class CraftTablePannel : InventoryPannel
    {
        public ItemSlot[] craftSlots;
        public ItemSlot resultSolt;

        protected override void Init()
        {
            _pannel.Canvas.sortingOrder = (int)UILayer.PopInventory;

            var index = 0;

            for (var i = 0; i < craftSlots.Length; ++i)
                craftSlots[i].Item = inventory.GetItem(index++);

            resultSolt.Item = inventory.GetItem(index);

            resultSolt.OnPut += (item, _) => false;
            resultSolt.OnTake += (item, count) =>
            {
                foreach (var itemSlot in craftSlots)
                {
                    if (itemSlot.Item.itemProxy == null)
                        continue;

                    if (itemSlot.Item.count >= count)
                        itemSlot.Item.count -= count;
                    else
                        return false;

                    if (itemSlot.Item.count == 0)
                        itemSlot.Item.itemProxy = null;
                }
                return true;
            };
        }

        protected override void Update()
        {
            base.Update();
            var itemList = new List<string>();
            foreach (var itemSlot in craftSlots)
                itemList.Add(itemSlot.Item.itemProxy?.Name ?? "");
            var craftProxy = RecipeBase.Get(
                CraftTableBehaviour.ToolName,
                RecipeType.Ordered,
                itemList.ToArray());
            if (craftProxy == null)
            {
                craftProxy = RecipeBase.Get(
                CraftTableBehaviour.ToolName,
                RecipeType.NonOrdered,
                itemList.ToArray());
            }

            if (craftProxy != null)
            {
                resultSolt.Item.itemProxy = craftProxy.Result;
                var minCount = int.MaxValue;
                foreach (var itemSlot in craftSlots)
                {
                    if (itemSlot.Item.itemProxy == null)
                        continue;
                    minCount = Mathf.Min(minCount, itemSlot.Item.count);
                }
                resultSolt.Item.count = minCount;
            }
            else
            {
                resultSolt.Item.itemProxy = null;
                resultSolt.Item.count = 0;
            }
        }
    }
}
