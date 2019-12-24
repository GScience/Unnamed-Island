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
    public class DropItemBehaviour : EntityBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        public Vector3 velocity;
        
        public IItem item;

        protected override void Init()
        {
            var spriteObject = new GameObject("sprite");
            spriteObject.transform.parent = transform;
            _spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteObject.AddComponent<Billboard>();
        }

        void OnSelected(PlayerBehaviour player)
        {
            _spriteRenderer.color = new Color(0.7f, 0.7f, 0.7f, 1);

            player.BindInteraction(KeyCode.F, "收集",
                new Action(() =>
                {
                    SendMessage("OnPlayerPickup", player);
                }));
        }

        void OnKill()
        {
            Destroy(gameObject);
        }

        void EntityUpdate()
        {
            // 检测是否在地上
            if (velocity.y <= float.Epsilon && Physics.Raycast(transform.position, Vector3.down, 0.5f, 1 << BlockContainer.Layer))
                velocity = Vector3.zero;
            else
            {
                velocity.y -= 10 * Time.deltaTime;
                transform.position += velocity * Time.deltaTime;
            }
        }

        void OnPlayerPickup(PlayerBehaviour player)
        {

        }

        void OnUnselected(PlayerBehaviour player)
        {
            _spriteRenderer.color = Color.white;
        }

        void EntityLoad(DataTag dataTag)
        {
            var itemName = dataTag.Get<string>("item");
            item = GameManager.ProxyManager.Get<IItem>(itemName);
            _spriteRenderer.sprite = item?.GetDropSprite();
            entity.IsSelectable = true;
            entity.HasUpdation = true;
            entity.SetCollider(
               Vector3.zero,
               0.5f);
            velocity = dataTag.Get<Vector3>("velocity");

            _spriteRenderer.material = GameManager.WorldManager.entityMaterial;
        }

        void EntitySave(DataTag dataTag)
        {
            dataTag.Set("item", item.Name);
            dataTag.Set("velocity", velocity);
        }
    }
}
