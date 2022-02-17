using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace PackageSystem
{
    [Serializable]
    public abstract class PackageContent
    {
        #region identification meta data
        [XmlElement()] private Guid guid = Guid.NewGuid();
        public Guid Guid { get { return guid; } }
        [XmlElement()] private Guid packageGuid;
        public string name = "Unnamed";
        public string groupName = "Misc";
        [XmlElement()] private SerializableTexture2D icon;
        [XmlIgnore] public virtual SerializableTexture2D Icon { get { return icon; } set { icon = value; } }
        #endregion

        #region creation metadata
        public DateTime creationTime;
        public string creatorName;
        #endregion
        [NonSerialized] private bool isDirty;
        public bool IsDirty { get { return isDirty; } }

        [XmlIgnore] public virtual Guid PackageGuid { get { return packageGuid; } set { packageGuid = value; } }
        [XmlIgnore] public virtual string FilePath { get { return PackageSystemPathVariables.FilePath(this); } }
        [XmlIgnore] public virtual string FolderPath { get { return PackageSystemPathVariables.FolderPath(this); } }

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
                package.RemoveEntry(this.GetType(), Guid);
                ResourceManager.RemoveAsset(this.GetType(), Guid);
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
            PackageGuid = manifest.Guid;
            RegisterToResourceManager();
        }

        public virtual void OnLoad(Guid packageGuid)
        {
            if (PackageGuid == Guid.Empty)
                PackageGuid = packageGuid;
        }
    }
}