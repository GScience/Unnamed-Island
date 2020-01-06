using island.Game.Extension.World;
using Island.Game.Proxy.EnvElements;
using Island.Game.System;
using Island.Game.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.EntityBehaviour
{
    public class EnvElementBehaviour : EntityBehaviour
    {
        private SpriteRenderer _spriteRenderer1;
        private SpriteRenderer _spriteRenderer2;

        public IEnvElement envElement;

        protected override void Init()
        {
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

        IEnumerator KillAnimaCoroutine(PlayerBehaviour player)
        {
            var alpha = 1f;
            
            while (alpha >= 0)
            {
                alpha -= Time.deltaTime * 5;

                _spriteRenderer1.color = 
                    new Color(
                        _spriteRenderer1.color.r, 
                        _spriteRenderer1.color.g, 
                        _spriteRenderer1.color.b, 
                        alpha);
                _spriteRenderer2.color =
                    new Color(
                        _spriteRenderer2.color.r,
                        _spriteRenderer2.color.g,
                        _spriteRenderer2.color.b,
                        alpha);

                _spriteRenderer1.transform.position += Vector3.down * Time.deltaTime;
                _spriteRenderer2.transform.position += Vector3.down * Time.deltaTime;

                yield return 1;
            }

            var entityChunk = Entity.GetChunk();
            if (entityChunk == null)
                yield break;

            entityChunk.CreateDropItem("island.item:dried_grass", 1, transform.position);

            Entity.BeKilled(player.Entity);
        }

        protected override void OnSelected(PlayerBehaviour player)
        {
            _spriteRenderer1.color = new Color(0.7f, 0.7f, 0.7f, 1);
            _spriteRenderer2.color = new Color(0.7f, 0.7f, 0.7f, 1);

            player.BindInteraction(KeyCode.P, "采集",
            new Action(() =>
            {
                SendMessage("OnPlayerCollect", player);
            }));
        }

        void OnPlayerCollect(PlayerBehaviour player)
        {
            StartCoroutine(KillAnimaCoroutine(player));
        }

        protected override void OnUnselected(PlayerBehaviour player)
        {
            _spriteRenderer1.color = Color.white;
            _spriteRenderer2.color = Color.white;
        }

        protected override void EntityLoad(DataTag dataTag)
        {
            var envElementName = dataTag.Get<string>("envElement");
            envElement = GameManager.ProxyManager.Get<IEnvElement>(envElementName);

            _spriteRenderer1.material = GameManager.WorldManager.entityMaterial;
            _spriteRenderer2.material = GameManager.WorldManager.entityMaterial;

            _spriteRenderer1.sprite = envElement?.GetEnvElementSprite();
            _spriteRenderer2.sprite = envElement?.GetEnvElementSprite();

            Entity.SetCollider(
                envElement?.GetColliderCenter() ?? Vector3.zero,
                envElement?.GetColliderSize() ?? 0);

            Entity.IsSelectable = true;
        }

        protected override void EntitySave(DataTag dataTag)
        {
            dataTag.Set("envElement", envElement.Name);
        }
    }
}
