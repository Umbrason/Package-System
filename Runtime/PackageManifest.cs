﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Serialization;

[System.Serializable]
///<summary> contains information about the packages content </summary>
public class PackageManifest : PackageContent
{
    public XmlDictionary<string, List<Guid>> packageContentGuids = new XmlDictionary<string, List<Guid>>();
    public List<Guid> dependencyGuids = new List<Guid>();

    [XmlIgnore]
    public override Guid PackageGuid { get { return guid; } set { guid = value; } }
    [XmlIgnore]
    public override string FilePath { get { return DirectoryPath + "/manifest" + EnginePathVariables.PackageManifestSuffix; } }
    [XmlIgnore]
    public string DirectoryPath { get { return EnginePathVariables.PackagePath + $"package-{base.guid}"; } }

    public PackageManifest()
    {
        this.guid = Guid.NewGuid();
    }

    public override void OnLoad(Guid packageGuid)
    {
        base.OnLoad(packageGuid);
    }

    public PackageManifest(string name = "new package")
    {
        this.name = name;
        this.guid = Guid.NewGuid();
    }


    public void RegisterEntry<T>(Guid guid) => RegisterEntry(typeof(T), guid);
    public void RegisterEntry(Type type, Guid guid)
    {
        if (packageContentGuids.TryGetValue(type.ToString(), out List<Guid> guidList))
            guidList.Add(guid);
        else
            packageContentGuids.Add(type.ToString(), new List<Guid> { guid });
        SetDirty();
    }

    public void RemoveEntry<T>(Guid guid) => RemoveEntry(typeof(T), guid);
    public void RemoveEntry(Type type, Guid guid)
    {
        if (packageContentGuids.TryGetValue(type.ToString(), out List<Guid> guidList))
            guidList.Remove(guid);
        SetDirty();
    }

    public bool ContainsEntry<T>(Guid guid) => ContainsEntry(typeof(T), guid);
    public bool ContainsEntry(Type type, Guid guid)
    {
        return packageContentGuids.TryGetValue(type.ToString(), out List<Guid> guidList) && guidList.Contains(guid);
    }
}
