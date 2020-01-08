using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Proxy.Items
{
    class Stone : ItemBase
    {
        public override string Name => "island.item:stone";

        protected override string GetDropSpriteName()
        {
            return "stone.drop";
        }

        protected override string GetItemSpriteName()
        {
            return "stone.item";
        }

        public override int GetMaxStackCount()
        {
            return 32;
        }
    }
}
