﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace PackageSystem
{
    [Serializable]
    public abstract class PackageContent
    {
        #region GUIDs
        private Guid guid = Guid.NewGuid();
        private Guid packageGuid;
        #endregion 

        #region meta data
        public string name = "Unnamed";
        public string groupName = "Misc";
        public string creatorName = "Unknown";
        public DateTime creationTime;
        private SerializableTexture2D icon;
        public virtual SerializableTexture2D Icon { get { return icon; } set { icon = value; } }
        #endregion

        [NonSerialized] private bool isDirty;

        #region properties
        public bool IsDirty { get { return isDirty; } }
        public Guid Guid { get { return guid; } }
        public virtual Guid PackageGuid { get { return packageGuid; } set { packageGuid = value; } }
        public virtual string FilePath { get { return PackageSystemPathVariables.FilePath(this); } }
        public virtual string FolderPath { get { return PackageSystemPathVariables.FolderPath(this); } }
        #endregion

        public PackageContent() { }

        ///<summary> equal to 'target != null' </summary>
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