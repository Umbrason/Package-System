using PackageSystem;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(PackageContentSpriteLibrary))]
public class PackageContentSpriteLibraryEditor : TypeObjectLibraryEditor<PackageContent, Sprite>
{
    public override bool IsValidType(Type type)
    {
        return type != typeof(PackageManifest);
    }

}
