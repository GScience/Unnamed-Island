using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.World
{
    /// <summary>
    /// 实体数据类型
    /// </summary>
    public class DataTag
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
        private static readonly Dictionary<int, Type> _dataTypeIndex = new Dictionary<int, Type> 
        {
            { typeof(Vector3).Name.GetHashCode(), typeof(Vector3) },
            { typeof(string).Name.GetHashCode(), typeof(string) },
            { typeof(int).Name.GetHashCode(), typeof(int) },
            { typeof(float).Name.GetHashCode(), typeof(float) }
        };

        /// <summary>
        /// 初始化数据加载器
        /// </summary>
        static DataTag()
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

            // int 32
            _dataLoader[typeof(int)] = new DataLoader(
                (BinaryWriter writer, object obj) =>
                {
                    var data = (int)obj;
                    writer.Write(data);
                },
                (BinaryReader reader) =>
                {
                    return reader.ReadInt32();
                });

            // float
            _dataLoader[typeof(float)] = new DataLoader(
                (BinaryWriter writer, object obj) =>
                {
                    var data = (float)obj;
                    writer.Write(data);
                },
                (BinaryReader reader) =>
                {
                    return reader.ReadSingle();
                });
        }

        /// <summary>
        /// 从二进制流中加载一个数据
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static object LoadValue(BinaryReader reader)
        {
            var typeIndex = reader.ReadInt32();
            if (!_dataTypeIndex.TryGetValue(typeIndex, out var type))
                Debug.LogError("Unknown type " + typeIndex);

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
            writer.Write(type.Name.GetHashCode()); 
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

        public DataTag(Dictionary<string, object> args = null)
        {
            if (args != null)
                foreach (var pair in args)
                    Set(pair.Key, pair.Value);
        }

        public static DataTag Empty()
        {
            return new DataTag();
        }
        #endregion
    }
}
