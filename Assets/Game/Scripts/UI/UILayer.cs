using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.UI
{
    /// <summary>
    /// 记录所有UI Canvas的渲染层次
    /// </summary>
    public enum UILayer : int
    {
        PlayerInfo = 1,
        Inventory = 2,

        WorldCreatingPannel = 101,
        GameLoadingPannel = 102,
        MessageBox = 201,

        DebugConsole = 901,
        GlobalFader=902,
        DebugInfo = 903
    }
}
