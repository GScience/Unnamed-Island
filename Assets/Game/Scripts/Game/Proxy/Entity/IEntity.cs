using Island.Game.GlobalEntity;
using Island.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Proxy.Entity
{
    public interface IEntity : IProxy
    {
        void Load(World.Entity entity, DataTag entityData);
        void Update(World.Entity entity);
        void SetColliderSize(World.Entity entity, SphereCollider collider);
        void OnSelectionChanged(World.Entity entity, bool selected);
        void Save(World.Entity entity, DataTag dataTag);
        bool HasUpdation { get; }
    }
}
