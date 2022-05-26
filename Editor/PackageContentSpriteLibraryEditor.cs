using System.Collections;
using System.Collections.Generic;
using PackageSystem;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PackageContentSpriteLibrary))]
public class PackageContentSpriteLibraryEditor : TypeObjectLibraryEditor<PackageContent,Sprite>{}
