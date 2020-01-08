using Island.Game.EntityBehaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Entities
{
    class CraftTable : IEntity
    {
        public string Name => "island.entity:craft_table";

        public Type[] EntityBehaviours =>
            new Type[]
            {
                typeof(CraftTableBehaviour)
            };

        public void Init()
        {
        }
    }
}
