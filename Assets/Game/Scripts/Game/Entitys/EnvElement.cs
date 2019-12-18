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
    [RequireComponent(typeof(SpriteRenderer), typeof(Billboard))]
    public class EnvElement : Entity
    {
        private SpriteRenderer _spriteRenderer;

        public IEnvElement envElement;

        public override bool IsSelected
        {
            set
            {
                if (value)
                    _spriteRenderer.color = new Color(0.7f, 0.7f, 0.7f, 1);
                else
                    _spriteRenderer.color = Color.white;
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
            _spriteRenderer.sprite = envElement?.GetEnvElementSprite();
            SetCollider(
                envElement?.GetColliderCenter() ?? Vector3.zero, 
                envElement?.GetColliderSize() ?? 0);

            HasUpdation = false;
        }

        private void Awake()
        {
            var spriteObject = new GameObject("sprite");
            spriteObject.transform.parent = transform;
            spriteObject.transform.localPosition = Vector3.zero;
            spriteObject.transform.localRotation = Quaternion.identity;
            _spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteObject.AddComponent<Billboard>();
        }
    }
}
