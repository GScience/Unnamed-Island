using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Render
{
    /// <summary>
    /// 用来显示BillBoard效果
    /// </summary>
    [ExecuteAlways]
    class Billboard : MonoBehaviour
    {
        void OnWillRenderObject()
        {
            transform.rotation = Camera.main.transform.rotation;
        }

        void Start()
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
