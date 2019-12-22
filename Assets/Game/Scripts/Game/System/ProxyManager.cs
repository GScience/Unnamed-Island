﻿using System;
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
        private readonly Dictionary<string, IProxy> _contentDict = new Dictionary<string, IProxy>();

        public void Register(Type dataType)
        {
            var data = (IProxy) Activator.CreateInstance(dataType);
            var fullName = data.Name.ToLower();
            Debug.Assert(!_contentDict.ContainsKey(fullName));
            _contentDict[fullName] = data;
        }

        public ProxyType Get<ProxyType>(string fullName) where ProxyType : IProxy
        {
            if (!_contentDict.TryGetValue(fullName.ToLower(), out var data))
                Debug.LogError("Data " + fullName + " not found in " + name);
            return (ProxyType) data;
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
                pair.Value.Init();
        }
    }
}
