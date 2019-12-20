using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Entity
{
    public class Player : IEntity
    {
        public string Name => "island.entity:player";

        public Type GetEntityType()
        {
            return typeof(EntityBehaviour.Player);
        }

        public void Init()
        {
        }
    }
}
