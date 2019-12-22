using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Entity
{
    class FallingItem : IEntity
    {
        public string Name => "island.entity:falling_item";

        public Type GetEntityType()
        {
            return typeof(EntityBehaviour.FallingItem);
        }

        public void Init()
        {
        }
    }
}
