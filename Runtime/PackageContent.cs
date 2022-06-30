using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

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
        private string creatorName = "Unknown";
        //private DateTime creationTime = DateTime.Now;
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
            Debug.Log($"saving {this.name} at {FilePath}");
            SerializationManager.Serialize(this, FilePath);
        }

        public bool TryRegisterToPackage(Guid guid)
        {
            if (!PackageManager.Instance.TryGetPackageManifest(PackageGuid, out PackageManifest manifest))
                return false;
            RegisterToPackage(manifest);
            return true;
        }

        public virtual void RegisterToPackage(PackageManifest manifest)
        {
            PackageGuid = manifest.guid;
            manifest.RegisterEntry(this.GetType(), guid);
        }



        public void Destroy()
        {
            if (PackageManager.Instance.TryGetPackageManifest(PackageGuid, out PackageManifest package))
            {
                package.RemoveEntry(this.GetType(), Guid);
                ResourceManager.RemoveAsset(this.GetType(), Guid);
                File.Delete(FilePath);
            }
        }

        public virtual void SetDirty()
        {
            if (isDirty)
                return;
            ResourceManager.EnqueueDirtyAsset(this);
            isDirty = true;
        }

        public virtual void OnCreation(PackageManifest manifest)
        {
            creatorName = "Umbrason";
            //creationTime = System.DateTime.Now;
            RegisterToPackage(manifest);
            ResourceManager.RegisterAsset(this);
            SetDirty();
        }

        public virtual void OnLoad(Guid packageGuid)
        {
            if (PackageGuid == Guid.Empty)
                PackageGuid = packageGuid;
        }
    }
}