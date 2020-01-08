using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Proxy.Items
{
    class FreshGrass : ItemBase
    {
        public override string Name => "island.item:fresh_grass";

        protected override string GetDropSpriteName()
        {
            return "fresh_grass.drop";
        }

        protected override string GetItemSpriteName()
        {
            return "fresh_grass.item";
        }

        public override int GetMaxStackCount()
        {
            return 32;
        }
    }
}
