using island.Game.World;
using Island.Game.Extension.World;
using Island.Game.System;
using Island.UI.Pannels.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Island.UI.Pannels.Player
{
    [RequireComponent(typeof(Pannel))]
    public class PlayerItemSelectPannel : MonoBehaviour, IPointerClickHandler
    {
        private Pannel _pannel;

        public Item selectedItem = new Item();
        public ItemSlot selectedItemSlot;

        void Awake()
        {
            _pannel = GetComponent<Pannel>();
            _instance = this;
        }

        private void Start()
        {
            _pannel.Canvas.sortingOrder = (int) UILayer.SelectedItem;
            selectedItemSlot.Item = selectedItem;
        }

        private void Update()
        {
            if (selectedItem.itemProxy == null)
                return;

            var rectSize = ((RectTransform)selectedItemSlot.transform).rect;

            selectedItemSlot.transform.position = Input.mousePosition + new Vector3(rectSize.x, -rectSize.y) / 3.0f;

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var result) && result.collider != null)
                {
                    var dropCount = Input.GetKey(KeyCode.Space) ? selectedItem.count : 1;

                    DropItem(result.point, dropCount);
                    selectedItem.count -= dropCount;
                    if (selectedItem.count <= 0)
                        selectedItem.itemProxy = null;
                }
            }
        }

        void DropItem(Vector3 dropTo, int dropCount)
        {
            var playerPos = GameManager.Player.transform.position;
            var dropDirection = dropTo - playerPos + Vector3.up * 0.5f;

            var dropVelocity = dropDirection.normalized * 5;

            GameManager.WorldManager.CreateDropItem(selectedItem.itemProxy.Name, dropCount, playerPos, dropVelocity);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        private static PlayerItemSelectPannel _instance;
        public static Item Item => _instance?.selectedItem;
    }
}
