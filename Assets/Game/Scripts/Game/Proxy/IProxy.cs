using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProxy
    {
        string Name { get; }

        void Init();
    }
}
