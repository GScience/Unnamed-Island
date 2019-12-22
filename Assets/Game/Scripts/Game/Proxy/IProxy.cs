using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy
{
    /// <summary>
    /// 代理相关
    /// 处理如获取实体类型、方块网格创建等
    /// </summary>
    public interface IProxy
    {
        string Name { get; }

        void Init();
    }
}
