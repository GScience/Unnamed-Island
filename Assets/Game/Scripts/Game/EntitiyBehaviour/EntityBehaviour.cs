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
        protected Entity entity;

        private void OnEntityCreated()
        {
            entity = GetComponent<Entity>();
            Init();
        }

        protected abstract void Init();
    }
}
