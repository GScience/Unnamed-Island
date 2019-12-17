﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Entitys
{
    /// <summary>
    /// 实体数据类型
    /// </summary>
    public class EntityData
    {
        #region DataLoader
        /// <summary>
        /// 实体数据类型加载器
        /// 负责处理特定类型的读取和保存
        /// </summary>
        private class DataLoader
        {
            public Action<BinaryWriter, object> save;
            public Func<BinaryReader, object> load;

            public DataLoader(Action<BinaryWriter, object> save, Func<BinaryReader, object> load)
            {
                this.save = save;
                this.load = load;
            }
        }

        /// <summary>
        /// 数据加载器索引
        /// </summary>
        private static readonly Dictionary<Type, DataLoader> _dataLoader = new Dictionary<Type, DataLoader>();

        /// <summary>
        /// 数据类型索引
        /// </summary>
        private static readonly Dictionary<string, Type> _dataTypeIndex = new Dictionary<string, Type> 
        {
            { typeof(Vector3).FullName, typeof(Vector3) },
            { typeof(string).FullName, typeof(string) }
        };

        /// <summary>
        /// 初始化数据加载器
        /// </summary>
        static EntityData()
        {
            // Vector3
            _dataLoader[typeof(Vector3)] = new DataLoader(
                (BinaryWriter writer, object obj) =>
                {
                    var data = (Vector3)obj;
                    writer.Write(data.x);
                    writer.Write(data.y);
                    writer.Write(data.z);
                },
                (BinaryReader reader) =>
                {
                    var x = reader.ReadSingle();
                    var y = reader.ReadSingle();
                    var z = reader.ReadSingle();

                    return new Vector3(x, y, z);
                });

            // string
            _dataLoader[typeof(string)] = new DataLoader(
                (BinaryWriter writer, object obj) =>
                {
                    var data = (string)obj;
                    writer.Write(data);
                },
                (BinaryReader reader) =>
                {
                    return reader.ReadString();
                });
        }

        /// <summary>
        /// 从二进制流中加载一个数据
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static object LoadValue(BinaryReader reader)
        {
            var typeName = reader.ReadString();
            if (!_dataTypeIndex.TryGetValue(typeName, out var type))
                Debug.LogError("Unknown type " + typeName);

            return _dataLoader[type].load(reader);
        }

        /// <summary>
        /// 写入数据到二进制流中
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="data"></param>
        private static void SaveValue(BinaryWriter writer, object data)
        {
            var type = data.GetType();
            writer.Write(type.FullName); 
            _dataLoader[type].save(writer, data);
        }

        #endregion

        #region EntityData

        /// <summary>
        /// 数据值
        /// </summary>
        private readonly Dictionary<string, object> _entityData = new Dictionary<string, object>();

        /// <summary>
        /// 获取值，失败则返回给定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T TryGet<T>(string key, T defaultValue)
        {
            if (_entityData.TryGetValue(key, out var value) && value.GetType() == typeof(T))
                return (T)value;
            return defaultValue;
        }

        /// <summary>
        /// 获取值，失败则返回默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (_entityData.TryGetValue(key, out var value) && value.GetType() == typeof(T))
                return (T)value;
            return default(T);
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public void Set(string key, object obj)
        {
            _entityData[key] = obj;
        }

        /// <summary>
        /// 保存EntityData到流中
        /// </summary>
        /// <param name="writer"></param>
        public void Save(BinaryWriter writer)
        {
            writer.Write((short)_entityData.Count);

            foreach (var pair in _entityData)
            {
                writer.Write(pair.Key);
                SaveValue(writer, pair.Value);
            }
        }

        /// <summary>
        /// 从流中加载EntityData
        /// </summary>
        /// <param name="reader"></param>
        public void Load(BinaryReader reader)
        {
            var count = reader.ReadInt16();

            for (var i = 0; i < count; ++i)
            {
                var key = reader.ReadString();
                var value = LoadValue(reader);
                _entityData[key] = value;
            }
        }

        public EntityData(string name, Type type, Dictionary<string, object> args = null)
        {
            Set("name", name);
            Set("type", type.FullName);

            if (args != null)
                foreach (var pair in args)
                    Set(pair.Key, pair.Value);
        }

        public string EntityName => Get<string>("name");
        public string EntityType => Get<string>("type");

        public static EntityData Empty()
        {
            return new EntityData("", typeof(Entity));
        }
        #endregion
    }
}
