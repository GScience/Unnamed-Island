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

namespace Island.UI.Pannels.Player
{
    [RequireComponent(typeof(Pannel))]
    public class PlayerInventoryPannel : MonoBehaviour
    {
        private Pannel _pannel;

        public ItemSlot[] playerBagSlots;
        public ItemSlot[] fastToolSelectSlots;

        public Image fastToolSelectorImage;

        public InventoryBehaviour inventory;

        private int _selectedItemIndex;

        void Awake()
        {
            _pannel = GetComponent<Pannel>();
        }

        private void Start()
        {
            _pannel.Canvas.sortingOrder = (int)UILayer.PlayerInventory;

            var itemIndex = 0;

            for (var i = 0; i < fastToolSelectSlots.Length; ++i)
                fastToolSelectSlots[i].Item = inventory.GetItem(itemIndex++);

            for (var i = 0; i < playerBagSlots.Length; ++i)
                playerBagSlots[i].Item = inventory.GetItem(itemIndex++);
        }

        void Update()
        {
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
