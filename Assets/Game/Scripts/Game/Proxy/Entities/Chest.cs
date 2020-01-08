using Island.Game.EntityBehaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Entities
{
    class Chest : IEntity
    {
        public string Name => "island.entity:chest";

        public Type[] EntityBehaviours =>
            new Type[]
            {
                typeof(ChestBehaviour)
            };

        public void Init()
        {
        }
    }
}
