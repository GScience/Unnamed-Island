using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Entities
{
    public interface IEntity : IProxy
    {
        Type[] EntityBehaviours { get; }
    }
}
