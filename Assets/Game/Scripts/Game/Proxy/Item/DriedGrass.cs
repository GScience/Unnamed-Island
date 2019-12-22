using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Proxy.Item
{
    class DriedGrass : ItemBase
    {
        public override string Name => "island.item:dried_grass";

        protected override string GetFallingSpriteName()
        {
            return "falling.dried_grass";
        }

        protected override string GetItemSpriteName()
        {
            return "item.dried_grass";
        }
    }
}
