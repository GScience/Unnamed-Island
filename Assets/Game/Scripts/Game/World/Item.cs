using Island.Game.Proxy.Items;
using Island.Game.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.World
{
    public class Item
    {
        public IItem itemProxy;
        public int count;

        public Item() { }
        public Item(string name, int count)
        {
            itemProxy = GameManager.ProxyManager.Get<IItem>(name);
            this.count = count;
        }
    }
}
