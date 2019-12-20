using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Proxy.EnvElements
{
    /// <summary>
    /// 环境元素数据接口
    /// </summary>
    public interface IEnvElement : IProxy
    {
        Sprite GetEnvElementSprite();
        float GetColliderSize();
        Vector3 GetColliderCenter();
    }
}
