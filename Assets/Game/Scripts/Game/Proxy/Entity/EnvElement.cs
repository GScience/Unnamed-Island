using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Proxy.EnvElements;
using Island.Game.System;
using Island.Game.World;
using UnityEngine;

namespace Island.Game.Proxy.Entity
{
    public class EnvElement : IEntity
    {
        public string Name => "island.entity:env_element";

        public void Init()
        {
        }

        public void Load(World.Entity entity, DataTag dataTag)
        {
            dynamic entityData = entity.GetEntityData();

            var sprite1Object = new GameObject("sprite1");
            var sprite2Object = new GameObject("sprite2");
            sprite1Object.transform.parent = entity.transform;
            sprite1Object.transform.localPosition = Vector3.zero;
            sprite1Object.transform.localRotation = Quaternion.AngleAxis(-45, Vector3.up);
            sprite2Object.transform.parent = entity.transform;
            sprite2Object.transform.localPosition = Vector3.zero;
            sprite2Object.transform.localRotation = Quaternion.AngleAxis(45, Vector3.up);
            entityData._spriteRenderer1 = sprite1Object.AddComponent<SpriteRenderer>();
            entityData._spriteRenderer2 = sprite2Object.AddComponent<SpriteRenderer>();

            var envElementName = dataTag.Get<string>("envElement");
            SetEnvElement(entity, envElementName);
        }

        public void SetEnvElement(World.Entity entity, string envElementName)
        {
            var entityData = entity.GetEntityData();
            var envElement = GameManager.ProxyManager.Get<IEnvElement>(envElementName);
            entityData.envElement = envElement;

            SpriteRenderer sprite1 = entityData._spriteRenderer1;
            SpriteRenderer sprite2 = entityData._spriteRenderer2;

            sprite1.sprite = envElement.GetEnvElementSprite();
            sprite2.sprite = envElement.GetEnvElementSprite();
        }

        public void Save(World.Entity entity, DataTag dataTag)
        {
            var entityData = entity.GetEntityData();
            dataTag.Set("envElement", (string) entityData.envElement.Name);
        }

        public void OnSelectionChanged(World.Entity entity, bool selected)
        {
            dynamic entityData = entity.GetEntityData();

            if (selected)
            {
                entityData._spriteRenderer1.color = new Color(0.7f, 0.7f, 0.7f, 1);
                entityData._spriteRenderer2.color = new Color(0.7f, 0.7f, 0.7f, 1);
            }
            else
            {
                entityData._spriteRenderer1.color = Color.white;
                entityData._spriteRenderer2.color = Color.white;
            }
        }

        public void SetColliderSize(World.Entity entity, SphereCollider collider)
        {
            var entityData = entity.GetEntityData();
            IEnvElement envElement = entityData.envElement;

            collider.radius = envElement.GetColliderSize();
            collider.center = envElement.GetColliderCenter();
        }

        public void Update(World.Entity entity)
        {
        }

        public bool HasUpdation => false;
    }
}
