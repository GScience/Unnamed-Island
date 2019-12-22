using Island.Game.EntityBehaviour;
using Island.Game.Proxy.Item;
using Island.Game.Render;
using Island.Game.System;
using Island.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.EntityBehaviour
{
    [RequireComponent(typeof(Entity))]
    public class FallingItem : MonoBehaviour
    {
        private Entity _entity;
        private SpriteRenderer _spriteRenderer;

        
        public IItem item;

        void Awake()
        {
            _entity = GetComponent<Entity>();

            var spriteObject = new GameObject("sprite");
            spriteObject.transform.parent = transform;
            _spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteObject.AddComponent<Billboard>();
        }

        void OnSelected(Player player)
        {
            _spriteRenderer.color = new Color(0.7f, 0.7f, 0.7f, 1);

            player.BindInteraction(KeyCode.F, "收集",
            new Action(() =>
            {
                SendMessage("OnPlayerPickup", player);
            }));
        }

        void EntityUpdate()
        {
            // 检测是否在地上
            if (Physics.Raycast(transform.position, Vector3.down, 0.5f, 1 << BlockContainer.Layer))
                return;

            transform.position += Vector3.down * Time.deltaTime;
        }

        void OnPlayerPickup(Player player)
        {

        }

        void OnUnselected(Player player)
        {
            _spriteRenderer.color = Color.white;
        }

        void EntityLoad(DataTag dataTag)
        {
            var itemName = dataTag.Get<string>("item");
            item = GameManager.ProxyManager.Get<IItem>(itemName);
            _spriteRenderer.sprite = item?.GetFallingSprite();
            _entity.IsSelectable = true;
            _entity.HasUpdation = true;
            _entity.SetCollider(
               Vector3.zero,
               0.5f);
        }

        void EntitySave(DataTag dataTag)
        {
            dataTag.Set("item", item.Name);
        }
    }
}
