using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Utils
{
    /// <summary>
    /// 用来储存每个场景所用到的Perfab
    /// </summary>
    public class PerfabsUtils : MonoBehaviour
    {
        private static PerfabsUtils _perfabsUtils;
        public List<GameObject> perfabs = new List<GameObject>();
        private Dictionary<string, GameObject> _perfabsDict = new Dictionary<string, GameObject>();

        private void Awake()
        {
            _perfabsUtils = this;

            foreach (var obj in perfabs)
                _perfabsDict[obj.name.ToLower()] = obj;
        }

        public static GameObject Create(string name)
        {
            if (_perfabsUtils == null)
                _perfabsUtils = FindObjectOfType<PerfabsUtils>();

            if (_perfabsUtils == null)
                Debug.LogError("There is no perfab registered to this scene");

            if (!_perfabsUtils._perfabsDict.TryGetValue(name.ToLower(), out var obj))
            {
                Debug.LogError("Failed to find perfab " + name);
                return null;
            }

            return Instantiate(obj);
        }
    }
}
