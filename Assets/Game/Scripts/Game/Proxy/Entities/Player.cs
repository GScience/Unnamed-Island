using Island.Game.EntityBehaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Entities
{
    public class Player : IEntity
    {
        public string Name => "island.entity:player";

        public Type[] EntityBehaviours =>
            new Type[]
            {
                typeof(PlayerBehaviour)
            };

        public void Init()
        {
        }
    }
}
