using Island.Game.EntityBehaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Entities
{
    public class EnvElement : IEntity
    {
        public string Name => "island.entity:env_element";

        public Type[] EntityBehaviours =>
            new Type[]
            {
                typeof(EnvElementBehaviour)
            };

        public void Init()
        {
        }
    }
}
