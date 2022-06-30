using UnityEngine;
using System;

namespace PackageSystem
{
    public abstract class PackageSubContent<T> : PackageContent where T : PackageContent, new()
    {
        public Guid parentGuid;
        public Guid parentPackageGuid;

        public Type ParentType { get { return typeof(T); } }
        public override string FilePath { get { return PackageSystemPathVariables.SubFilePath(this); } }
        public override string FolderPath { get { return PackageSystemPathVariables.SubFolderPath(this); } }
        [NonSerialized] private T cached_parent;
        public T Parent
        {
            get
            {
                if (cached_parent)
                    return cached_parent;
                if (PackageManager.Instance.TryGetPackageManifest(parentPackageGuid, out PackageManifest manifest))
                    return cached_parent = ResourceManager.LoadAsset<T>(manifest, parentGuid);
                Debug.LogError($"Parent ({ParentType}:{parentGuid}) of ({GetType()}:{name}-{Guid}) not found!");
                return null;
            }
        }

        public override void OnLoad(Guid packageGuid) => base.OnLoad(packageGuid);
        public PackageSubContent() { }
        public PackageSubContent(Guid parentGuid) { this.parentGuid = parentGuid; }
        public override void RegisterToPackage(PackageManifest manifest) { }
    }
}