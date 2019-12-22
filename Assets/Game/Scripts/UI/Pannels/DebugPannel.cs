using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.UI.Pannels
{
    [RequireComponent(typeof(Pannel))]
    class DebugPannel : MonoBehaviour
    {
        private Pannel _pannel;

        private void Awake()
        {
            _pannel = GetComponent<Pannel>();
        }

        private void Start()
        {
            _pannel.Canvas.sortingOrder = (int)UILayer.DebugInfo;
        }
    }
}
