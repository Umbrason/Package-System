using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace PackageSystem
{
    [System.Serializable]
    public abstract class PackageContent
    {
        public Guid guid = Guid.NewGuid();

        private bool isDirty;
        public bool IsDirty { get { return isDirty; } }

        public string name = "Unnamed";
        public string groupName = "Misc";
        private SerializableTexture2D icon;
        public virtual SerializableTexture2D Icon { get { return icon; } set { icon = value; } }

        [XmlIgnore]
        public virtual string FilePath { get { return PackageSystemPathVariables.FilePath(this); } }
        public virtual string FolderPath { get { return PackageSystemPathVariables.FolderPath(this); } }

        private Guid packageGuid;
        [XmlIgnore]
        public virtual Guid PackageGuid { get { return packageGuid; } set { packageGuid = value; } }
        public DateTime creationTime;
        public string creatorName;

        public PackageContent() { }

        //handy little bool operator overload
        public static implicit operator bool(PackageContent value) => value != null;


        public virtual void SaveToDisk()
        {
            isDirty = false;
            SerializationManager.Serialize(this, FilePath);
        }

        public virtual void RegisterToPackage(Guid guid)
        {
            PackageGuid = guid;
            if (PackageManager.Instance.TryGetPackageManifest(PackageGuid, out PackageManifest manifest))
                manifest.RegisterEntry(this.GetType(), guid);
        }

        public virtual void RegisterToResourceManager()
        {
            ResourceManager.RegisterAsset(this, this.GetType());
        }

        public virtual void RemoveFromPackage()
        {
            if (PackageManager.Instance.TryGetPackageManifest(PackageGuid, out PackageManifest package))
            {
                package.RemoveEntry(this.GetType(), guid);
                ResourceManager.RemoveAsset(this.GetType(), guid);
            }
        }

        public virtual void SetDirty()
        {
            if (isDirty)
                return;
            ResourceManager.RegisterDirtyAsset(this);
            isDirty = true;
        }

        public virtual void OnCreation(PackageManifest manifest)
        {
            creatorName = "Umbrason";
            creationTime = System.DateTime.Now;
            PackageGuid = manifest.guid;
            RegisterToResourceManager();
        }

        public virtual void OnLoad(Guid packageGuid)
        {
            if (PackageGuid == Guid.Empty)
                PackageGuid = packageGuid;
        }
    }
}