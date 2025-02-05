﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.World;
using UnityEngine;

namespace Island.Game.Proxy.EnvElements
{
    public class WitheredGrass : EnvElementBase
    {
        public override string Name => "island.env_element:withered_grass";

        protected override string GetSpriteName()
        {
            return "withered_grass";
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
            return new Item("island.item:dried_grass", 1);
        }
    }
}
