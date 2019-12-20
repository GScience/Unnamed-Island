using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Proxy.EnvElements
{
    public class WitheredGrass : EnvElementBase
    {
        public override string Name => "island.env_element:withered_grass";

        protected override string GetSpriteName()
        {
            return "witheredGrass";
        }

        public override float GetColliderSize()
        {
            return 0.6f;
        }

        public override Vector3 GetColliderCenter()
        {
            return new Vector3(0, 0.7f, 0);
        }
    }
}
