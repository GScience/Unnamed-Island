﻿using Island.Game.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Data.EnvElements
{
    public abstract class EnvElementBase : IEnvElement
    {
        private Sprite _sprite;

        public abstract string Name { get; }

        public Sprite GetEnvElementSprite()
        {
            return _sprite;
        }

        protected abstract string GetSpriteName();

        public void Load()
        {
            _sprite = GameManager.EnvElementSpriteDatabase.Get(GetSpriteName());
        }
    }
}
