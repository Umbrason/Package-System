using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

public static class EnginePathVariables
{
    ///<summary>Persistent path containing various data such as save-files and packages</summary>
    public static string DataPath { get { return Application.isEditor ? $"\\\\?\\{Path.GetFullPath($"{Application.dataPath.Replace("/", "\\")}\\..\\ApplicationData")}" : Application.persistentDataPath; } }


    ///<summary>Path containing all packages including '\' at the end</summary>
    public static string PackagePath { get { return DataPath + "\\Packages\\"; } }

    ///<summary>File ending for all package manifest files including the '.'</summary>
    public static string PackageManifestSuffix { get { return ".pamf"; } }


    /////<summary>Path containing all save files including '\' at the end</summary>
    //public static string SaveFileSubPath { get { return DataPath + "/Saves/"; } }
    ///<summary>File ending for all save files including the '.'</summary>
    public static string SaveFileSuffix { get { return ".save"; } }

    /////<summary>Path containing all area files including '\' at the end</summary>
    //public static string AreaFileSubPath { get { return DataPath + "/Areas/"; } }

    ///<summary>File ending for all area files including the '.'</summary>
    public static string AreaFileSuffix { get { return ".area"; } }

    ///<summary>Generic method returning a path containing all serialized files of type 'T' including '\' at the end</summary>
    public static string FolderPath(PackageContent target) => $"{PackagePath}package-{target.PackageGuid}\\{target.GetType().Name}\\";

    public static string SubFolderPath<T>(PackageSubContent<T> target) where T : PackageContent, new()
        => SubFolderPath(target.Parent, target.GetType());
    public static string SubFolderPath(PackageContent parent, Type subType)
        => $"{parent.FolderPath}{parent.guid}\\{subType.Name}\\";


    ///<summary>Generic method returning a file suffix for all files of type 'T' including '.'</summary>
    public static string FileSuffix<T>() => FileSuffix(typeof(T));
    ///<summary>Non-Generic method returning a file suffix for all files of type 'T' including '.'</summary>
    public static string FileSuffix(Type type) => $".{FileSuffixNoDot(type)}";


    ///<summary>Generic method returning a file suffix for all files of type 'T' NOT including '.'</summary>
    public static string FileSuffixNoDot<T>() => FileSuffixNoDot(typeof(T));
    ///<summary>Non-Generic method returning a file suffix for all files of type 'T' NOT including '.'</summary>
    public static string FileSuffixNoDot(Type type) => $"{string.Concat(type.Name.Where(c => c >= 'A' && c <= 'Z'))}";

    public static string FilePath(PackageContent target)
        => FilePath(target.PackageGuid, target.guid, target.GetType());
    public static string FilePath(PackageManifest manifest, Guid contentGuid, Type contentType)
        => FilePath(manifest.guid, contentGuid, contentType);
    public static string FilePath(Guid packageGuid, Guid contentGuid, Type type)
        => $"{PackagePath}package-{packageGuid}\\{type.Name}\\{contentGuid}{FileSuffix(type)}";

    public static string SubFilePath<T>(PackageSubContent<T> target) where T : PackageContent, new()
        => SubFilePath(target.Parent, target.guid, target.GetType());
    public static string SubFilePath(PackageContent parent, Guid subGuid, Type subType)
        => $"{parent.FolderPath}{parent.guid}\\{subType.Name}\\{subGuid}{FileSuffix(subType)}";
}
