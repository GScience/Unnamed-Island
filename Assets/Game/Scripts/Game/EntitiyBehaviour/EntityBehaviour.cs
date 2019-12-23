using Island.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.EntitiyBehaviour
{
    public class EntityBehaviour : MonoBehaviour
    {
        private Entity _entity;

        protected virtual void Awake()
        {
            _entity = GetComponent<Entity>();
        }
    }
}
