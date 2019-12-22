using Island.Game.Proxy.EnvElements;
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
    public class EnvElement : MonoBehaviour
    {
        private Entity _entity;
        private SpriteRenderer _spriteRenderer1;
        private SpriteRenderer _spriteRenderer2;

        public IEnvElement envElement;

        void Awake()
        {
            _entity = GetComponent<Entity>();

            var sprite1Object = new GameObject("sprite1");
            var sprite2Object = new GameObject("sprite2");
            sprite1Object.transform.parent = transform;
            sprite1Object.transform.localPosition = Vector3.zero;
            sprite1Object.transform.localRotation = Quaternion.AngleAxis(-45, Vector3.up);
            sprite2Object.transform.parent = transform;
            sprite2Object.transform.localPosition = Vector3.zero;
            sprite2Object.transform.localRotation = Quaternion.AngleAxis(45, Vector3.up);
            _spriteRenderer1 = sprite1Object.AddComponent<SpriteRenderer>();
            _spriteRenderer2 = sprite2Object.AddComponent<SpriteRenderer>();
        }

        void OnSelected(Player player)
        {
            _spriteRenderer1.color = new Color(0.7f, 0.7f, 0.7f, 1);
            _spriteRenderer2.color = new Color(0.7f, 0.7f, 0.7f, 1);

            player.BindInteraction(KeyCode.F, "采集",
            new Action(() =>
            {
                SendMessage("OnPlayerCollect", player);
            }));
        }

        void OnPlayerCollect(Player player)
        {
            var entityChunk = _entity.GetChunk();
            if (entityChunk == null)
                return;

            entityChunk.CreateEntity(new DataTag(
                new Dictionary<string, object>
                {
                    {"type", "island.entity:falling_item" },
                    {"name", "fallingItem" },
                    {"position", transform.position + Vector3.up },
                    {"item", "island.item:dried_grass" }
                }));
            _entity.Kill();
        }

        void OnUnselected(Player player)
        {
            _spriteRenderer1.color = Color.white;
            _spriteRenderer2.color = Color.white;
        }

        void EntityLoad(DataTag dataTag)
        {
            var envElementName = dataTag.Get<string>("envElement");
            envElement = GameManager.ProxyManager.Get<IEnvElement>(envElementName);
            _spriteRenderer1.sprite = envElement?.GetEnvElementSprite();
            _spriteRenderer2.sprite = envElement?.GetEnvElementSprite();
            _entity.SetCollider(
                envElement?.GetColliderCenter() ?? Vector3.zero,
                envElement?.GetColliderSize() ?? 0);

            _entity.IsSelectable = true;
        }

        void EntitySave(DataTag dataTag)
        {
            dataTag.Set("envElement", envElement.Name);
        }
    }
}
