﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Entity
{
    public class EnvElement : IEntity
    {
        public string Name => "island.entity:env_element";

        public Type GetEntityType()
        {
            return typeof(EntityBehaviour.EnvElement);
        }

        public void Init()
        {
        }
    }
}
