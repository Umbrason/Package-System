using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

namespace PackageSystem
{
    public static class PackageSerializationHelper
    {
        public static void SerializePackageContent(PackageContent target, string filePath) => Serialize(target, filePath);

        public static bool TryDeserializePackageContent<T>(string filePath, out T target) where T : PackageContent, new() => TryDeserialize<T>(filePath, out target);

        public static bool TryDeserializePackageContent(string filePath, Type type, out PackageContent target)
        {
            if (TryDeserialize(filePath, type, out object objectTarget) && objectTarget is PackageContent)
            {
                target = (PackageContent)objectTarget;
                return true;
            }
            target = default;
            return false;
        }

        public static void Serialize(object target, string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            FileStream file;
            if (File.Exists(filePath))
                file = File.OpenWrite(filePath);
            else file = File.Create(filePath);
            file.SetLength(0);
            var factory = new XmlSerializerFactory();
            var serializer = factory.CreateSerializer(target.GetType());
            serializer.Serialize(file, target);
            file.Close();
        }

        public static bool TryDeserialize<T>(string filePath, out T target)
        {
            if (TryDeserialize(filePath, typeof(T), out object data) && data is T)
            {
                target = (T)data;
                return true;
            }
            target = default;
            return false;
        }

        public static bool TryDeserialize(string filePath, Type type, out object target)
        {
            if (File.Exists(filePath))
            {
                var file = File.Open(filePath, FileMode.Open);
                var factory = new XmlSerializerFactory();
                var serializer = factory.CreateSerializer(type);
                object data = serializer.Deserialize(file);
                file.Close();
                target = Convert.ChangeType(data, type);
                if (target != null)
                    return true;
            }
            target = default;
            return false;
        }

        public async static Task<T> DeserializeAsync<T>(string filePath) where T : new()
        {
            StreamReader reader = File.OpenText(filePath);

            string text = await reader.ReadToEndAsync();

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var stringReader = new StringReader(text);
            object data = serializer.Deserialize(stringReader);
            reader.Close();

            return (T)data;
        }

        public async static Task<PackageContent> DeserializePackageContentAsync(string filePath, Type type)
        {
            var reader = File.OpenText(filePath);
            var text = await reader.ReadToEndAsync();
            var serializer = new System.Xml.Serialization.XmlSerializer(type);
            TextReader stringReader = new StringReader(text);
            object data = serializer.Deserialize(stringReader);
            reader.Close();

            return (PackageContent)data;
        }
    }
}