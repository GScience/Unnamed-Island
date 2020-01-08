using Island.Game.EntityBehaviour;
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
        }
    }
}
