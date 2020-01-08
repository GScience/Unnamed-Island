using Island.Game.World;
using Island.Game.EntityBehaviour;
using Island.UI.Pannels.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Island.UI.Pannels.Inventory
{
    [RequireComponent(typeof(Pannel))]
    public class PlayerInventoryPannel : InventoryPannel
    {
        public ItemSlot[] playerBagSlots;
        public ItemSlot[] fastToolSelectSlots;

        public Image fastToolSelectorImage;

        private int _selectedItemIndex;

        protected override void Init()
        {
            _pannel.Canvas.sortingOrder = (int)UILayer.PlayerInventory;

            var itemIndex = 0;

            for (var i = 0; i < fastToolSelectSlots.Length; ++i)
                fastToolSelectSlots[i].Item = inventory.GetItem(itemIndex++);

            for (var i = 0; i < playerBagSlots.Length; ++i)
                playerBagSlots[i].Item = inventory.GetItem(itemIndex++);
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.RightArrow))
                _selectedItemIndex++;
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                _selectedItemIndex--;

            if (_selectedItemIndex >= fastToolSelectSlots.Length)
                _selectedItemIndex = 0;
            if (_selectedItemIndex < 0)
                _selectedItemIndex = fastToolSelectSlots.Length - 1;

            fastToolSelectorImage.rectTransform.position = fastToolSelectSlots[_selectedItemIndex].GetComponent<RectTransform>().position;
        }
    }
}
