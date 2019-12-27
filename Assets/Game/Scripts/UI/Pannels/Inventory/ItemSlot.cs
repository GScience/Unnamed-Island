using island.Game.World;
using Island.Game.EntityBehaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Island.UI.Pannels.Inventory
{
    public class ItemSlot : MonoBehaviour
    {
        public Text itemCountIext;
        public Image itemImage;

        public Item Item { get; private set; }

        public InventoryBehaviour inventory;
        public int slot;

        private void Update()
        {
            Item = inventory?.GetItem(slot);

            itemCountIext.text = "" + Item?.count ?? "";
            itemImage.sprite = Item?.itemProxy?.GetItemSprite() ?? null;
            if (itemImage.sprite == null)
                itemImage.color = new Color(0, 0, 0, 0);
            else
                itemImage.color = new Color(1, 1, 1, 1);
        }
    }
}
