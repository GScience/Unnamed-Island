using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Island.Game.Proxy;
using UnityEditor;
using UnityEngine;

namespace Island.Game.System
{
    /// <summary>
    /// 游戏内容管理器，记录包括包括方块、物品类型等信息。
    /// </summary>
    public class ProxyManager : MonoBehaviour
    {
        private readonly Dictionary<string, Dictionary<string, IProxy>> _contentDict 
            = new Dictionary<string, Dictionary<string, IProxy>>();

        public void Register(Type dataType)
        {
            var data = (IProxy) Activator.CreateInstance(dataType);
            var fullName = data.Name.ToLower();
            var namespaceName = fullName.Substring(0, fullName.IndexOf(':'));
            var name = fullName.Substring(fullName.IndexOf(':') + 1);
            if (!_contentDict.ContainsKey(namespaceName))
                _contentDict[namespaceName] = new Dictionary<string, IProxy>();
            var dict = _contentDict[namespaceName];
            Debug.Assert(!dict.ContainsKey(name));
            dict[name] = data;
        }

        public ProxyType Get<ProxyType>(string fullName) where ProxyType : IProxy
        {
            var namespaceName = fullName.Substring(0, fullName.IndexOf(':'));
            var name = fullName.Substring(fullName.IndexOf(':') + 1);
            if (!_contentDict.TryGetValue(namespaceName, out var dict))
                Debug.LogError("Data " + fullName + " not found in " + this.name);
            if (!dict.TryGetValue(name, out var data))
                Debug.LogError("Data " + fullName + " not found in " + this.name);
            return (ProxyType) data;
        }

        public bool TryGet<ProxyType>(string fullName, out ProxyType proxy) where ProxyType : IProxy
        {
            fullName = fullName.ToLower();
            var namespaceName = fullName.Substring(0, fullName.IndexOf(':'));
            var name = fullName.Substring(fullName.IndexOf(':') + 1);

            if (!_contentDict.TryGetValue(namespaceName, out var dict))
            {
                proxy = default;
                return false;
            }

            if (!dict.TryGetValue(name, out var data))
            {
                proxy = default;
                return false;
            }
            proxy = (ProxyType)data;
            return true;
        }

        void Awake()
        {
            foreach (var type in GetType().Assembly.GetTypes())
            {
                if (!typeof(IProxy).IsAssignableFrom(type) || type.IsAbstract)
                    continue;

                Register(type);
            }
        }

        void Start()
        {
            foreach (var pair in _contentDict)
                foreach (var pair2 in pair.Value)
                    pair2.Value.Init();
        }
    }
}
