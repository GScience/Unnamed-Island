using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.EntityBehaviour;
using Island.Game.System;
using UnityEngine;

namespace Island.Game.World
{
    /// <summary>
    /// 世界锚
    /// 用于自动加载Chunk
    /// </summary>
    [RequireComponent(typeof(Entity))]
    public class Anchor : MonoBehaviour
    {
    }
}
