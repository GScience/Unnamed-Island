using Island.Game.Data.EnvElements;
using Island.Game.Render;
using Island.Game.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Entitys
{
    /// <summary>
    /// 环境元素实体
    /// </summary>
    public class EnvElement : Entity
    {
        private SpriteRenderer _spriteRenderer1;
        private SpriteRenderer _spriteRenderer2;

        public IEnvElement envElement;

        public override bool IsSelected
        {
            set
            {
                if (value)
                {
                    _spriteRenderer1.color = new Color(0.7f, 0.7f, 0.7f, 1);
                    _spriteRenderer2.color = new Color(0.7f, 0.7f, 0.7f, 1);
                }
                else
                {
                    _spriteRenderer1.color = Color.white;
                    _spriteRenderer2.color = Color.white;
                }
            }
        }

        protected override void UpdateEntityState()
        {
        }
        protected override void SaveToEntityData()
        {
            base.SaveToEntityData();
            _entityData.Set("envElement", envElement.Name);
        }

        protected override void LoadFromEntityData()
        {
            base.LoadFromEntityData();
            var envElementName = _entityData.Get<string>("envElement");
            envElement = GameManager.DataManager.Get<IEnvElement>(envElementName);
            _spriteRenderer1.sprite = envElement?.GetEnvElementSprite(); 
            _spriteRenderer2.sprite = envElement?.GetEnvElementSprite();
            SetCollider(
                envElement?.GetColliderCenter() ?? Vector3.zero, 
                envElement?.GetColliderSize() ?? 0);

            HasUpdation = false;
        }

        private void Awake()
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

            //spriteObject.AddComponent<Billboard>();
        }
    }
}
