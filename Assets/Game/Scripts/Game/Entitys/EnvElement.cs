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
    [RequireComponent(typeof(SpriteRenderer), typeof(Billboard))]
    public class EnvElement : Entity
    {
        private SpriteRenderer _spriteRenderer;
        public IEnvElement envElement;

        protected override void UpdateMovement()
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
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
