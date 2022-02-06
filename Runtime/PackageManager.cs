using System.Collections;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PackageSystem
{
    public class PackageManager : MonoBehaviour
    {
        private static PackageManager singleton;
        public static PackageManager Instance { get { return singleton; } }

        public PackageManifest[] packageManifests { get { return currentManifests != null ? currentManifests.Values.ToArray() : new PackageManifest[0]; } }

        private Dictionary<Guid, PackageManifest> currentManifests = new Dictionary<Guid, PackageManifest>();
        public bool TryGetPackageManifest(Guid guid, out PackageManifest manifest) => currentManifests.TryGetValue(guid, out manifest);

        public UnityEvent OnReloadManifests = new UnityEvent();



        public void Awake()
        {
            if (singleton != null && singleton != this)
            {
                Destroy(singleton.gameObject);
            }
            DontDestroyOnLoad(gameObject);
            singleton = this;
            FetchManifests();
        }

        public void SavePackage(PackageManifest manifest)
        {

        }


        public void FetchManifests()
        {
            if (!Directory.Exists(PackageSystemPathVariables.PackagePath))
                Directory.CreateDirectory(PackageSystemPathVariables.PackagePath);

            string[] PackageManifestFiles = Directory.GetFiles(PackageSystemPathVariables.PackagePath, $"*{PackageSystemPathVariables.PackageManifestSuffix}", SearchOption.AllDirectories);
            List<PackageManifest> manifests = new List<PackageManifest>();
            foreach (string filePath in PackageManifestFiles)
            {
                if (PackageSerializationHelper.TryDeserializePackageContent<PackageManifest>(filePath, out PackageManifest manifest))
                {
                    manifests.Add(manifest);
                }
                else
                    Debug.LogWarning($"{filePath} could not be deserialized");
            }
            currentManifests.Clear();
            foreach (PackageManifest manifest in manifests)
                currentManifests.Add(manifest.guid, manifest);

            OnReloadManifests.Invoke();
        }
    }
}