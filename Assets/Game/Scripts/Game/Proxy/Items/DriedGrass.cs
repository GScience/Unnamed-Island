using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Proxy.Items
{
    class DriedGrass : ItemBase
    {
        public override string Name => "island.item:dried_grass";

        protected override string GetDropSpriteName()
        {
            return "dried_grass.drop";
        }

        protected override string GetItemSpriteName()
        {
            return "dried_grass.item";
        }

        public override int GetMaxStackCount()
        {
            return 32;
        }
    }
}
