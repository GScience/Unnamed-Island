using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IData
    {
        string Name { get; }

        void Load();
    }
}
