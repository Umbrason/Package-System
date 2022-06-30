using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

namespace PackageSystem
{
    public static class PackageSystemPathVariables
    {
        private const string LONGPATH_PREFIX = "\\\\?\\";
        ///<summary>Persistent path containing various data such as save-files and packages</summary>
        private static string DataPath { get { return Application.isEditor ? $"{Path.GetFullPath($"{Application.dataPath.Replace("/", "\\")}\\..\\ApplicationData")}" : Application.persistentDataPath; } }

        ///<summary>Path containing all packages including '\' at the end</summary>
        public static string PackagePath { get { return DataPath + "\\Packages\\"; } }

        ///<summary>Generic method returning a path containing all serialized files of type 'T' including '\' at the end</summary>
        public static string FolderPath(PackageContent target) => $"{PackagePath}package-{target.PackageGuid}\\{target.GetType().Name}\\";

        public static string SubFolderPath<T>(PackageSubContent<T> target) where T : PackageContent, new()
            => SubFolderPath(target.Parent, target.GetType());
        public static string SubFolderPath(PackageContent parent, Type subType)
            => $"{parent.FolderPath}{parent.Guid}\\{subType.Name}\\";


        ///<summary>Generic method returning a file suffix for all files of type 'T' including '.'</summary>
        public static string DefaultFileSuffix<T>() => DefaultFileSuffix(typeof(T));
        ///<summary>Non-Generic method returning a file suffix for all files of type 'T' including '.'</summary>
        public static string DefaultFileSuffix(Type type) => $".{DefaultFileSuffixNoDot(type)}";


        ///<summary>Generic method returning a file suffix for all files of type 'T' NOT including '.'</summary>
        public static string DefaultFileSuffixNoDot<T>() => DefaultFileSuffixNoDot(typeof(T));
        ///<summary>Non-Generic method returning a file suffix for all files of type 'T' NOT including '.'</summary>
        public static string DefaultFileSuffixNoDot(Type type) => type == typeof(PackageManifest) ? "packagemanifest" : $"{string.Concat(type.Name.Where(c => c >= 'A' && c <= 'Z'))}";                

        public static string FilePath(PackageContent target)
            => FilePath(target.PackageGuid, target.Guid, target.GetType());
        public static string FilePath(PackageManifest manifest, Guid contentGuid, Type contentType)
            => FilePath(manifest.Guid, contentGuid, contentType);
        public static string FilePath(Guid packageGuid, Guid contentGuid, Type type)
            => $"{PackagePath}package-{packageGuid}\\{type.Name}\\{contentGuid}{DefaultFileSuffix(type)}";

        public static string SubFilePath<T>(PackageSubContent<T> target) where T : PackageContent, new()
            => SubFilePath(target.Parent, target.Guid, target.GetType());
        public static string SubFilePath(PackageContent parent, Guid subGuid, Type subType)
            => $"{parent.FolderPath}{parent.Guid}\\{subType.Name}\\{subGuid}{DefaultFileSuffix(subType)}";
    }
}