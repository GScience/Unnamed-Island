using Island.Game.EntityBehaviour;
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

        public Type[] EntityBehaviours =>
            new Type[]
            {
                typeof(GameCameraBehaviour)
            };

        public void Init()
        {
        }
    }
}
