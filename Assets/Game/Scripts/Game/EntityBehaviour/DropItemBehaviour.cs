using Island.Game.World;
using Island.Game.EntityBehaviour;
using Island.Game.Proxy.Items;
using Island.Game.Render;
using Island.Game.System;
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
        private Vector3 accelerate;

        public Item item = new Item();

        private InventoryBehaviour _pickupBy;

        protected override void Init()
        {
            var spriteObject = new GameObject("sprite");
            spriteObject.transform.parent = transform;
            _spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteObject.AddComponent<Billboard>();
        }

        protected override void OnSelected(PlayerBehaviour player)
        {
            _spriteRenderer.color = new Color(0.7f, 0.7f, 0.7f, 1);

            player.BindInteraction(KeyCode.P, "收集",
                new Action(() =>
                {
                    SendMessage("OnPlayerPickup", player);
                }));
        }

        protected override void EntityUpdate()
        {
            _spriteRenderer.transform.localPosition = new Vector3(
                0, 
                Mathf.Sin((Time.time + gameObject.GetInstanceID() / 10.0f) * 2) * 0.1f, 
                0);

            // 计算速度和拾取
            if (_pickupBy != null)
            {
                var distPos = _pickupBy.Entity.transform.position + Vector3.up * 0.5f;

                // 和玩家重合后捡起物品
                if (Vector3.Distance(distPos, transform.position) < 0.75f)
                {
                    _spriteRenderer.color = 
                        new Color(
                            _spriteRenderer.color.r,
                            _spriteRenderer.color.g, 
                            _spriteRenderer.color.b, 
                            _spriteRenderer.color.a - 5 * Time.deltaTime);

                    // 捡到物品
                    if (_spriteRenderer.color.a <= float.Epsilon)
                    {
                        _pickupBy.AddItem(ref item);
                        if (item.count == 0)
                        {
                            Entity.BeKilled(_pickupBy.Entity);
                            return;
                        }
                        _pickupBy = null;
                        _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 1);
                    }
                }
                else
                {
                    // 继续计算速度
                    velocity = Vector3.SmoothDamp(velocity, (distPos - transform.position) * 5, ref accelerate, 0.1f);
                    _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 1);
                }
            }
            else
            {
                // 超出范围不受物理影响
                if (Vector3.Distance(transform.position, GameManager.Player.transform.position) > 5 && velocity.y <= float.Epsilon)
                    velocity.y = 0;
                else
                {
                    // 物品受重力影响掉落
                    if (velocity.y <= float.Epsilon && Physics.Raycast(transform.position, Vector3.down, 0.5f, 1 << BlockContainer.Layer))
                        velocity = Vector3.zero;
                    else
                        velocity.y -= 10 * Time.deltaTime;
                }
            }

            // 运动
            transform.position += velocity * Time.deltaTime;
        }

        protected void OnPlayerPickup(PlayerBehaviour player)
        {
            _pickupBy = player.Inventory;
        }

        protected override void OnUnselected(PlayerBehaviour player)
        {
            _spriteRenderer.color = Color.white;
        }

        protected override void EntityLoad(DataTag dataTag)
        {
            var itemName = dataTag.Get<string>("item");
            item.itemProxy = GameManager.ProxyManager.Get<IItem>(itemName);
            item.count = dataTag.Get<int>("itemCount");
            _spriteRenderer.sprite = item.itemProxy?.GetDropSprite();
            Entity.IsSelectable = true;
            Entity.HasUpdation = true;
            Entity.SetCollider(
               Vector3.zero,
               0.5f);
            velocity = dataTag.Get<Vector3>("velocity");

            _spriteRenderer.material = GameManager.WorldManager.entityMaterial;
        }

        protected override void EntitySave(DataTag dataTag)
        {
            dataTag.Set("item", item?.itemProxy?.Name ?? "");
            dataTag.Set("itemCount", item?.count ?? 0);
            dataTag.Set("velocity", velocity);
        }
    }
}
