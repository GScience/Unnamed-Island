using island.Game.World;
using Island.Game.EntityBehaviour;
using Island.UI.Pannels.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.UI.Pannels.Player
{
    [RequireComponent(typeof(Pannel))]
    public class PlayerInventoryPannel : MonoBehaviour
    {
        private Pannel _pannel;

        public ItemSlot[] itemSlots;

        public InventoryBehaviour inventory;

        void Awake()
        {
            _pannel = GetComponent<Pannel>();
        }

        private void Start()
        {
            _pannel.Canvas.sortingOrder = (int)UILayer.PlayerInventory;

            for (var i = 0; i < itemSlots.Length; ++i)
                itemSlots[i].Item = inventory.GetItem(i);
        }
    }
}
