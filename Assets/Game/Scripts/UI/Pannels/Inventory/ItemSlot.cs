using island.Game.World;
using Island.Game.EntityBehaviour;
using Island.UI.Pannels.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Island.UI.Pannels.Inventory
{
    public class ItemSlot : MonoBehaviour, IPointerClickHandler
    {
        public Text itemCountIext;
        public Image itemImage;

        public Item Item { get; set; }

        private void Update()
        {
            if (Item?.count == 0)
                itemCountIext.text = "";
            else
                itemCountIext.text = "" + Item?.count ?? "";

            itemImage.sprite = Item?.itemProxy?.GetItemSprite() ?? null;
            if (itemImage.sprite == null)
                itemImage.color = new Color(0, 0, 0, 0);
            else
                itemImage.color = new Color(1, 1, 1, 1);
        }

        /// <summary>
        /// 点击物品框
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (Item == null)
                return;

            var selectedItem = PlayerItemSelectPannel.Item;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (selectedItem.itemProxy == Item.itemProxy)
                    TakeAll();
                else
                    Swap();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (selectedItem.itemProxy == Item.itemProxy || Item.itemProxy == null)
                {
                    if (Input.GetKey(KeyCode.Space))
                        PutAll();
                    else
                        PutOne();
                }
            }
        }

        /// <summary>
        /// 和手中物品交换
        /// </summary>
        void Swap()
        {
            var selectedItemProxy = PlayerItemSelectPannel.Item.itemProxy;
            var selectedItemCount = PlayerItemSelectPannel.Item.count;
            PlayerItemSelectPannel.Item.itemProxy = Item.itemProxy;
            PlayerItemSelectPannel.Item.count = Item.count;
            Item.itemProxy = selectedItemProxy;
            Item.count = selectedItemCount;
        }

        /// <summary>
        /// 拿走一个
        /// </summary>
        void TakeOne()
        {
            if (Item.itemProxy == null)
                return;

            var selectedItem = PlayerItemSelectPannel.Item;

            if (selectedItem.itemProxy != null && selectedItem.itemProxy != Item.itemProxy)
                return;

            selectedItem.itemProxy = Item.itemProxy;

            if (selectedItem.count >= Item.itemProxy.GetMaxStackCount() || Item.count <= 0)
                return;

            Item.count--;

            if (Item.count <= 0)
            {
                Item.count = 0;
                Item.itemProxy = null;
            }

            selectedItem.count++;
        }

        /// <summary>
        /// 拿走所有
        /// </summary>
        void TakeAll()
        {
            if (Item.itemProxy == null)
                return;

            var selectedItem = PlayerItemSelectPannel.Item;

            if (selectedItem.itemProxy != null && selectedItem.itemProxy != Item.itemProxy)
                return;

            selectedItem.itemProxy = Item.itemProxy;

            if (selectedItem.count >= Item.itemProxy.GetMaxStackCount() || Item.count <= 0)
                return;

            selectedItem.count += Item.count;

            if (selectedItem.count >= Item.itemProxy.GetMaxStackCount())
            {
                Item.count = selectedItem.count - Item.itemProxy.GetMaxStackCount();
                selectedItem.count = Item.itemProxy.GetMaxStackCount();
            }
            else
                Item.count = 0;

            if (Item.count <= 0)
            {
                Item.count = 0;
                Item.itemProxy = null;
            }
        }

        /// <summary>
        /// 放置一个
        /// </summary>
        void PutOne()
        {
            var selectedItem = PlayerItemSelectPannel.Item;

            if (selectedItem.itemProxy == null)
                return;

            if (Item.itemProxy != null && Item.itemProxy != selectedItem.itemProxy)
                return;

            if (Item.count >= selectedItem.itemProxy.GetMaxStackCount() || selectedItem.count <= 0)
                return;

            Item.itemProxy = selectedItem.itemProxy;

            selectedItem.count--;

            if (selectedItem.count <= 0)
            {
                selectedItem.count = 0;
                selectedItem.itemProxy = null;
            }

            Item.count++;
        }

        /// <summary>
        /// 放置所有
        /// </summary>
        void PutAll()
        {
            var selectedItem = PlayerItemSelectPannel.Item;

            if (selectedItem.itemProxy == null)
                return;

            if (Item.itemProxy != null && Item.itemProxy != selectedItem.itemProxy)
                return;

            if (Item.count >= selectedItem.itemProxy.GetMaxStackCount() || selectedItem.count <= 0)
                return;

            Item.itemProxy = selectedItem.itemProxy;
            Item.count += selectedItem.count;

            if (Item.count >= selectedItem.itemProxy.GetMaxStackCount())
            {
                selectedItem.count = Item.count - selectedItem.itemProxy.GetMaxStackCount();
                Item.count = selectedItem.itemProxy.GetMaxStackCount();
            }
            else
                selectedItem.count = 0;

            if (selectedItem.count <= 0)
            {
                selectedItem.count = 0;
                selectedItem.itemProxy = null;
            }
        }
    }
}
