using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Proxy.Item
{
    public interface IItem : IProxy
    {
        Sprite GetDropSprite();
        Sprite GetItemSprite();
    }
}
