using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Entity
{
    class DropItem : IEntity
    {
        public string Name => "island.entity:drop_item";

        public Type GetEntityType()
        {
            return typeof(EntityBehaviour.DropItem);
        }

        public void Init()
        {
        }
    }
}
