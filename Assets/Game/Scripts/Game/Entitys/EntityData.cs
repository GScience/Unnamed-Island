using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Island.Game.Entitys
{
    public class EntityData
    {
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

        private static readonly Dictionary<Type, DataLoader> _dataLoader = new Dictionary<Type, DataLoader>();
        private static readonly Dictionary<string, Type> _dataTypeIndex = new Dictionary<string, Type> 
        {
            { typeof(Vector3).FullName, typeof(Vector3) },
            { typeof(string).FullName, typeof(string) }
        };

        static EntityData()
        {
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

        private static object LoadValue(BinaryReader reader)
        {
            var typeName = reader.ReadString();
            if (!_dataTypeIndex.TryGetValue(typeName, out var type))
                Debug.LogError("Unknown type " + typeName);

            return _dataLoader[type].load(reader);
        }

        private static void SaveValue(BinaryWriter writer, object data)
        {
            var type = data.GetType();
            writer.Write(type.FullName); 
            _dataLoader[type].save(writer, data);
        }

        private readonly Dictionary<string, object> _entityData = new Dictionary<string, object>();

        public T TryGet<T>(string key, T defaultValue)
        {
            if (_entityData.TryGetValue(key, out var value) && value.GetType() == typeof(T))
                return (T)value;
            return defaultValue;
        }

        public T Get<T>(string key)
        {
            if (_entityData.TryGetValue(key, out var value) && value.GetType() == typeof(T))
                return (T)value;
            return default(T);
        }

        public void Set(string key, object obj)
        {
            _entityData[key] = obj;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write((short)_entityData.Count);

            foreach (var pair in _entityData)
            {
                writer.Write(pair.Key);
                SaveValue(writer, pair.Value);
            }
        }

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

        public static EntityData Empty = new EntityData("", typeof(Entity));
    }
}
