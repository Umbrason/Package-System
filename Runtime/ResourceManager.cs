using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PackageSystem
{
    public static class ResourceManager
    {
        private static Dictionary<Type, Dictionary<Guid, PackageContent>> loadedResources = new Dictionary<Type, Dictionary<Guid, PackageContent>>();
        private static Queue<PackageContent> dirtyAssets = new Queue<PackageContent>();

        #region Asset Getters
        public static T[] GetAllLoadedAssets<T>() where T : PackageContent => GetAllLoadedAssets(typeof(T)).Cast<T>().ToArray();
        public static PackageContent[] GetAllLoadedAssets(Type type)
        {
            Dictionary<Guid, PackageContent> dict = GetLoadedContentDict(type);
            return dict.Values.ToArray();
        }

        public static Guid[] GetAllLoadedAssetGuids<T>() where T : PackageContent
        {
            Dictionary<Guid, PackageContent> dict = GetLoadedContentDict(typeof(T));
            Guid[] keys = dict.Keys.ToArray();
            return keys;
        }

        public static T GetAsset<T>(Guid guid) where T : PackageContent => (T)GetAsset(typeof(T), guid);
        public static PackageContent GetAsset(Type type, Guid guid)
        {
            if (TryGetAsset(guid, type, out PackageContent asset))
                return asset;
            return default;
        }

        public static bool TryGetAsset<T>(Guid guid, out T asset) where T : PackageContent => asset = TryGetAsset(guid, typeof(T), out PackageContent c) ? (T)c : null;
        public static bool TryGetAsset(Guid guid, Type type, out PackageContent asset)
        {
            if (loadedResources.TryGetValue(type, out Dictionary<Guid, PackageContent> contentDictionary) && contentDictionary.TryGetValue(guid, out PackageContent content))
            {
                asset = content;
                return true;
            }
            asset = default;
            return false;
        }
        #endregion

        #region Manual Resource Management
        public static void RegisterAsset<T>(T asset) where T : PackageContent => RegisterAsset(asset, typeof(T));
        public static void RegisterAsset(PackageContent asset, Type type)
        {
            Dictionary<Guid, PackageContent> contentDictionary = GetLoadedContentDict(type);
            if (!contentDictionary.ContainsKey(asset.guid))
                contentDictionary.Add(asset.guid, asset);
        }

        public static void RemoveAsset<T>(Guid guid) => RemoveAsset(typeof(T), guid);
        public static void RemoveAsset(Type type, Guid guid)
        {
            Dictionary<Guid, PackageContent> contentDictionary = GetLoadedContentDict(type);
            if (contentDictionary.ContainsKey(guid))
                contentDictionary.Remove(guid);
        }
        #endregion

        #region Saving of Assets

        public static void RegisterDirtyAsset(PackageContent asset) => dirtyAssets.Enqueue(asset);
        public static void SaveSingleAsset(PackageContent content)
        {
            content.SaveToDisk();
            dirtyAssets = new Queue<PackageContent>(dirtyAssets.Where((x) => x.IsDirty));
        }
        public static void SaveAll()
        {
            while (dirtyAssets.Count > 0)
                dirtyAssets.Dequeue().SaveToDisk();
        }

        #endregion

        #region Loading of Assets
        public static T LoadAsset<T>(PackageManifest package, Guid guid) where T : PackageContent, new() => (T)LoadAsset(package, guid, typeof(T));
        public static PackageContent LoadAsset(PackageManifest package, Guid guid, Type type)
        {
            if (TryGetAsset(guid, type, out PackageContent asset))
                return asset;
            Dictionary<Guid, PackageContent> contentDictionary = GetLoadedContentDict(type);
            asset = DeserializeAssetInPackage(package, guid, type);
            asset.OnLoad(package.guid);
            contentDictionary.Add(asset.guid, asset);
            return asset;
        }

        public static K LoadSubAsset<T, K>(PackageContent parentAsset, Guid subAssetGuid) where K : PackageContent, new() where T : PackageContent, new() => (K)LoadSubAsset(parentAsset, subAssetGuid, typeof(K));
        public static PackageContent LoadSubAsset(PackageContent parentAsset, Guid subGuid, Type subType)
        {
            if (TryGetAsset(subGuid, subType, out PackageContent asset))
                return asset;
            Dictionary<Guid, PackageContent> contentDictionary = GetLoadedContentDict(subType);
            asset = DeserializeSubAsset(parentAsset, subGuid, subType);
            if (!asset)
            {
                Debug.LogWarning($"Subasset {subGuid} of type {subType} could not be found!");
                return null;
            }
            asset.OnLoad(parentAsset.PackageGuid);
            contentDictionary[asset.guid] = asset;
            return asset;
        }

        public static T[] LoadAllAssetsOfType<T>(PackageManifest package) where T : PackageContent, new() => LoadAllAssetsOfType(package, typeof(T)).Cast<T>().ToArray();
        public static PackageContent[] LoadAllAssetsOfType(PackageManifest package, Type type)
        {
            if (!package.packageContentGuids.ContainsKey(type.ToString()))
                return new PackageContent[0];
            var loadedContentDictionary = GetLoadedContentDict(type);
            var wasteGuids = new Queue<Guid>();
            foreach (Guid guid in package.packageContentGuids[type.ToString()])
            {
                if (!loadedContentDictionary.ContainsKey(guid))
                {
                    PackageContent asset = DeserializeAssetInPackage(package, guid, type);
                    if (asset == null)
                    {
                        Debug.LogWarning($"Asset {guid} of type {type} could not be found!");
                        wasteGuids.Enqueue(guid);
                        continue;
                    }
                    asset.OnLoad(package.guid);
                    loadedContentDictionary.Add(asset.guid, asset);
                }
            }
            if (wasteGuids.Count > 0)
                Debug.Log($"attempting to remove the following GUIDs {SerializationUtil.IEnumerableToString(wasteGuids)} of type {type}");
            while (wasteGuids.Count > 0)
                package.RemoveEntry(type, wasteGuids.Dequeue());
            return loadedContentDictionary.Values.ToArray();
        }

        public static S[] LoadAllSubAssets<S>(PackageContent parentAsset) where S : PackageContent, new() => LoadAllSubAssets(parentAsset, typeof(S)).Cast<S>().ToArray();
        public static PackageContent[] LoadAllSubAssets(PackageContent parentAsset, Type subType)
        {
            PackageContent[] subAssets = DeserializeAllSubAssets(parentAsset, subType);
            foreach (PackageContent subAsset in subAssets)
            {
                RegisterAsset(subAsset);
            }
            return subAssets;
        }


        public static async Task<T> LoadAsync<T>(PackageManifest package, Guid guid) where T : PackageContent, new()
        {
            if (package.packageContentGuids.TryGetValue(typeof(T).ToString(), out List<Guid> Guids))
            {
                string filePath = PackageSystemPathVariables.FilePath(package, guid, typeof(T));
                T asset = await PackageSerializationHelper.DeserializeAsync<T>(filePath);
                Debug.Log("loaded Asset");
                return asset;
            }
            Debug.Log($"couldnt find {guid} of type {typeof(T)}");
            return null;
        }
        public static async void LoadAllAssetsAsync<T>(PackageManifest package, System.Action<T> OnLoadCallback, System.Action<T[]> OnFinishedLoading) where T : PackageContent, new()
        {
            Dictionary<Guid, PackageContent> contentDictionary = GetLoadedContentDict(typeof(T));
            foreach (Guid guid in package.packageContentGuids[typeof(T).ToString()])
            {
                if (!contentDictionary.ContainsKey(guid))
                {
                    Task<T> task = LoadAsync<T>(package, guid);
                    await task;
                    T asset = task.Result;
                    asset.OnLoad(package.guid);
                    contentDictionary.Add(asset.guid, asset);
                    OnLoadCallback.Invoke(asset);
                }
            }
            OnFinishedLoading.Invoke((T[])contentDictionary.Values.ToArray());
        }
        #endregion

        #region private getter functions
        private static Dictionary<Guid, PackageContent> GetLoadedContentDict(Type type)
        {
            if (!loadedResources.TryGetValue(type, out Dictionary<Guid, PackageContent> contentDictionary))
            {
                contentDictionary = new Dictionary<Guid, PackageContent>();
                loadedResources.Add(type, contentDictionary);
            }
            return contentDictionary;
        }
        #endregion

        #region Deserialization
        private static T DeserializeAssetAtPath<T>(string path) where T : PackageContent, new() => (T)DeserializeAssetAtPath(path, typeof(T));
        private static PackageContent DeserializeAssetAtPath(string path, Type type)
        {
            if (PackageSerializationHelper.TryDeserializePackageContent(path, type, out PackageContent asset))
            {
                return asset;
            }
            return null;
        }

        private static T DeserializeAssetInPackage<T>(PackageManifest package, Guid guid) where T : PackageContent, new() => (T)DeserializeAssetInPackage(package, guid, typeof(T));
        private static PackageContent DeserializeAssetInPackage(PackageManifest package, Guid guid, Type type)
        {
            string path = PackageSystemPathVariables.FilePath(package, guid, type);
            if (PackageSerializationHelper.TryDeserializePackageContent(path, type, out PackageContent asset))
            {
                return asset;
            }
            return null;
        }

        private static async Task<T> DeserializeAssetAtPathAsync<T>(string path) where T : PackageContent, new()
        {
            T asset = await PackageSerializationHelper.DeserializeAsync<T>(path);
            if (asset != null)
            {
                return asset;
            }
            return null;
        }
        private static async Task<PackageContent> DeserializeAssetAtPathAsync(string path, Type type)
        {
            PackageContent asset = await PackageSerializationHelper.DeserializePackageContentAsync(path, type);
            if (asset != null)
            {
                return asset;
            }
            return null;
        }

        private static PackageSubContent<T> DeserializeSubAsset<T>(PackageContent parentAsset, Guid subAssetGuid) where T : PackageContent, new() => (PackageSubContent<T>)DeserializeSubAsset(parentAsset, subAssetGuid, typeof(T));
        private static PackageContent DeserializeSubAsset(PackageContent parentAsset, Guid subGuid, Type subType)
        {
            foreach (PackageManifest m in PackageManager.Instance.packageManifests)
            {
                string path = PackageSystemPathVariables.SubFilePath(parentAsset, subGuid, subType);
                PackageContent content = DeserializeAssetAtPath(path, subType);
                if (content)
                    return content;
                Debug.Log($"asset not found at path {path}");
            }
            return null;
        }

        private static T[] DeserializeAllSubAssets<T>(PackageContent parentAsset) where T : PackageContent, new() => DeserializeAllSubAssets(parentAsset, typeof(T)).Cast<T>().ToArray();
        private static PackageContent[] DeserializeAllSubAssets(PackageContent parentAsset, Type subAssetType)
        {
            var subAssets = new List<PackageContent>();
            foreach (PackageManifest m in PackageManager.Instance.packageManifests)
            {
                var directoryPath = PackageSystemPathVariables.SubFolderPath(parentAsset, subAssetType);
                if (!Directory.Exists(directoryPath))
                    continue;
                var files = Directory.GetFiles(directoryPath, $"*{PackageSystemPathVariables.FileSuffix(subAssetType)}");
                Debug.Log($"{directoryPath} contains {SerializationUtil.IEnumerableToString(files)}");
                foreach (var path in files)
                {
                    var asset = DeserializeAssetAtPath(path, subAssetType);
                    if (asset != null)
                        subAssets.Add(asset);
                }
            }
            return subAssets.ToArray();
        }
        #endregion
    }
}