using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.System
{
    /// <summary>
    /// 游戏内容管理器，包括方块类型等信息。每个存档的游戏内容不保证一致
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        private readonly Dictionary<string, IData> _contentDict = new Dictionary<string, IData>();

        public void Register(Type dataType)
        {
            var data = (IData) Activator.CreateInstance(dataType);
            var fullName = data.Name.ToLower();
            Debug.Assert(!_contentDict.ContainsKey(fullName));
            _contentDict[fullName] = data;
        }

        public TDataType Get<TDataType>(string fullName) where TDataType : IData
        {
            if (!_contentDict.TryGetValue(fullName.ToLower(), out var data))
                Debug.LogError("Data " + fullName + " not found");
            return (TDataType) data;
        }

        void Awake()
        {
            foreach (var type in GetType().Assembly.GetTypes())
            {
                if (!typeof(IData).IsAssignableFrom(type) || type.IsAbstract)
                    continue;

                Register(type);
            }
        }

        void Start()
        {
            foreach (var pair in _contentDict)
                pair.Value.Load();
        }
    }
}
