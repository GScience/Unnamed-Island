using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Entity
{
    public class GameCamera : IEntity
    {
        public string Name => "island.entity:game_camera";

        public Type GetEntityType()
        {
            return typeof(EntityBehaviour.GameCamera);
        }

        public void Init()
        {
        }
    }
}
