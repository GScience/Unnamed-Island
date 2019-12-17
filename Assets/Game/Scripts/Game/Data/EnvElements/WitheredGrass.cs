using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Data.EnvElements
{
    public class WitheredGrass : EnvElementBase
    {
        public override string Name => "island.env_element:withered_grass";

        protected override string GetSpriteName()
        {
            return "witheredGrass";
        }
    }
}
