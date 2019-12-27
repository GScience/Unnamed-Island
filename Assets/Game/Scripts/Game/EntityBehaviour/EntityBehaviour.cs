using Island.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.EntityBehaviour
{
    public abstract class EntityBehaviour : MonoBehaviour
    {
        public Entity Entity { get; private set; }

        protected T GetEntityBehaviour<T>() where T : EntityBehaviour
        {
            return Entity.GetComponent<T>();
        }

        protected void OnEntityCreated()
        {
            Entity = GetComponent<Entity>();
            Init();
        }

        /// <summary>
        /// 在实体以及行为创建完成后调用，此时未加载数据
        /// </summary>
        protected abstract void Init();
        protected abstract void EntityLoad(DataTag dataTag);
        protected abstract void EntitySave(DataTag dataTag);

        protected virtual void EntityUpdate()
        {
        }

        protected virtual void OnUnselected(PlayerBehaviour player)
        {
        }

        protected virtual void OnSelected(PlayerBehaviour player)
        {
        }

        protected virtual void OnKill(Entity killBy)
        {
        }

        protected virtual void OnAttack(Entity attackBy)
        {
        }
    }
}
