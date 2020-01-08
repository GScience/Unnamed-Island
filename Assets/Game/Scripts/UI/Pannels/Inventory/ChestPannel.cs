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
    public class ChestPannel : InventoryPannel
    {
        public ItemSlot[] itemSlots;

        protected override void Init()
        {
            _pannel.Canvas.sortingOrder = (int)UILayer.PopInventory;

            for (var i = 0; i < itemSlots.Length; ++i)
                itemSlots[i].Item = inventory.GetItem(i);
        }
    }
}
