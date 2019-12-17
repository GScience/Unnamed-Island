using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Data.EnvElements
{
    /// <summary>
    /// 环境元素数据接口
    /// </summary>
    public interface IEnvElement : IData
    {
        Sprite GetEnvElementSprite();
    }
}
