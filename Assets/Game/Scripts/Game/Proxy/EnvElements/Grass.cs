using Island.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Proxy.EnvElements
{
    public class Grass : EnvElementBase
    {
        public override string Name => "island.env_element:grass";

        protected override string GetSpriteName()
        {
            return "grass";
        }

        public override float GetColliderSize()
        {
            return 0.6f;
        }

        public override Vector3 GetColliderCenter()
        {
            return new Vector3(0, 0.7f, 0);
        }

        public override Item GetFallingItem()
        {
            return new Item("island.item:fresh_grass", 1);
        }
    }
}
