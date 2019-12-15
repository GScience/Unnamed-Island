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
            { typeof(Vector3).FullName, typeof(Vector3) } 
        };

        static EntityData()
        {
            _dataLoader[typeof(Vector3)] = new DataLoader(
                (BinaryWriter writer, object obj) =>
                {
                    var data = (Vector3) obj;
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
        }

        private static object LoadValue(BinaryReader reader)
        {
            var typeName = reader.ReadString();
            var type = _dataTypeIndex[typeName];

            return _dataLoader[type].load(reader);
        }

        private static void SaveValue(BinaryWriter writer, object data)
        {
            var type = data.GetType();
            writer.Write(type.FullName); 
            _dataLoader[type].save(writer, data);
        }

        private readonly Dictionary<string, object> _entityData = new Dictionary<string, object>();

        public Vector3 TryGetVector3(string key, Vector3 defaultValue)
        {
            if (_entityData.TryGetValue(key, out var value))
                return (Vector3)value;
            return defaultValue;
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
    }
}
