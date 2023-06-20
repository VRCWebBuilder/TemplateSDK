using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VRC.SDK3.Avatars.Components;
using System.Text.RegularExpressions;
using System.IO;
using VRC.Core;
using VRC.SDKBase;
using System.IO.Compression;
using Codice.Client.BaseCommands;

public class TemplateBuilder : EditorWindow
{   
    public TemplateDesc.Customization AvatarCustomizationOptions;

    Vector2 scrollPos;
    GameObject AvatarGO;

    VRCAvatarDescriptor AviDesc;
    PipelineManager AviManager;

    //webgl
    GameObject TempGO;
    PipelineManager TempManager;
    VRCAvatarDescriptor TempDesc;

    [MenuItem("Avatar Template/(2) Compile Template")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TemplateBuilder));
    }

    private void OnGUI()
    {
        //Scrolling
        //EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        //GUILayout.Label(TFLogo);
        //GUI.DrawTexture(new Rect(10, 10, 200, 60), TFLogo, ScaleMode.ScaleToFit, true, 3.0F);
        //GUILayout.Space(75f);
        GUILayout.Label("Compile a ready-made avatar into a web\ncustomizable form for anyone to use.");
        //Array field
        ScriptableObject scriptableObj = this;
        SerializedObject serialObj = new SerializedObject(scriptableObj);
        SerializedProperty serial = serialObj.FindProperty("AvatarCustomizationOptions");

        EditorGUILayout.PropertyField(serial, true);
        serialObj.ApplyModifiedProperties();
        //-----

        GUILayout.Space(10);

        if (GUILayout.Button("Compile"))
        {
            CompileAsset();
        }

        EditorGUILayout.EndScrollView();
    }

    public void CompileAsset()
    {
        BuildTarget[] platforms = { BuildTarget.StandaloneWindows, BuildTarget.Android, BuildTarget.WebGL };

        AssetDatabase.CreateFolder("Assets/TemplateSDK/Templates", "Temp");

        string path = "Assets/TemplateSDK/Templates/Temp/data.txt";

        StreamWriter writer = new StreamWriter(path, true);

        writer.WriteLine(AvatarCustomizationOptions.Name);

        writer.Close();

        //Re-import the file to update the reference in the editor

        AssetDatabase.ImportAsset(path);

        AssetDatabase.Refresh();

        for (int i = 0; i < platforms.Length; i++)
        {
            string ProperName = AvatarCustomizationOptions.Avatar.gameObject.name;
            AvatarCustomizationOptions.Avatar.gameObject.name = "Avatar";

            TemplateDesc Template = AvatarCustomizationOptions.Avatar.gameObject.AddComponent<TemplateDesc>();
            Template.Template = AvatarCustomizationOptions;
            Template.ViewPos = AvatarCustomizationOptions.Avatar.ViewPosition;

            //List<object> Assets = new List<object>();

            string localPath = "Assets/TemplateSDK/Templates/Temp/Avatar.prefab";

            // Make sure the file name is unique, in case an existing Prefab has the same name.
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

            AvatarGO = AvatarCustomizationOptions.Avatar.gameObject;

            //GameObject Clone = Instantiate(AvatarGO);

            //Clone.name = AvatarGO.name;

            if (platforms[i] == BuildTarget.WebGL)
            {
                 TempGO = new GameObject();
                 TempDesc = TempGO.AddComponent<VRCAvatarDescriptor>();
                 TempManager = TempGO.AddComponent<PipelineManager>();


                TempDesc.GetCopyOf(AvatarGO.GetComponent<PipelineManager>());
                TempManager.GetCopyOf(AvatarGO.GetComponent<VRCAvatarDescriptor>());

                DestroyImmediate(AvatarGO.GetComponent<PipelineManager>());
                DestroyImmediate(AvatarGO.GetComponent<VRCAvatarDescriptor>());
            }

            bool prefabSuccess;
            PrefabUtility.SaveAsPrefabAsset(AvatarGO, localPath, out prefabSuccess);
            /*if (prefabSuccess)
                Debug.Log("<color=green>Prefab was saved successfully!</color> (Now mark it with the correct assetbundle)");
*/

            //PrefabAssetType Prefab = (PrefabAssetType)AssetDatabase.LoadAssetAtPath("Assets/AviTemplateSDK/Templates/" + AvatarCustomizationOptions.Name + ".prefab", typeof(PrefabAssetType)) as PrefabAssetType;

            //var importer = UnityEditor.AssetImporter.GetAtPath("Assets/AviTemplateSDK/Templates/" + AvatarCustomizationOptions.Name + ".prefab");
            //importer.assetBundleName = "Template";

            AssetBundleBuild[] Buildmap = new AssetBundleBuild[1];
            string ValidName = Regex.Replace(AvatarCustomizationOptions.Name, "[^\\w\\._]", "");
            Buildmap[0].assetBundleName = ValidName + "Template";
            Buildmap[0].assetBundleVariant = "Template";
            Buildmap[0].assetNames = new string[] { "Assets/TemplateSDK/Templates/Temp/Avatar.prefab" };

            //BuildPipeline.BuildAssetBundle(, , "Assets/AviTemplateSDK/Templates/", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

            //OLD
            //BuildPipeline.BuildAssetBundles("Assets/TemplateSDK/Templates/Temp/", Buildmap, BuildAssetBundleOptions.None, BuildTarget.WebGL);

            DestroyImmediate(AvatarGO.GetComponent<TemplateDesc>());

            AvatarGO.name = ProperName;

            //PrefabUtility.UnpackPrefabInstance(AvatarCustomizationOptions.Avatar.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

            //"Assets/AviTemplateSDK/Templates/" + AvatarCustomizationOptions.Name, Buildmap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows

            AssetImporter A = AssetImporter.GetAtPath("Assets/TemplateSDK/Templates/Temp/Avatar.prefab");
            A.assetBundleName = platforms[i].ToString().ToLower();
            A.assetBundleVariant = "template";
            AssetDatabase.ImportAsset("Assets/TemplateSDK/Templates/Temp/Avatar.prefab", ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
            BuildAllAssetBundles(platforms[i]);
        }

        string ValidName2 = Regex.Replace(AvatarCustomizationOptions.Name, "[^\\w\\._]", "");
        ZipFile.CreateFromDirectory("Assets/TemplateSDK/Templates/Temp", "Assets/TemplateSDK/Templates/Package.zip");

        foreach (UnityEngine.Object Asset in AssetDatabase.LoadAllAssetsAtPath("Assets/TemplateSDK/Templates/Temp/"))
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(Asset));
        }
        AssetDatabase.DeleteAsset("Assets/TemplateSDK/Templates/Temp");

        //AssetDatabase.RenameAsset("Assets/TemplateSDK/Templates/Package.zip", ValidName2 + ".template");
        File.Move("Assets/TemplateSDK/Templates/Package.zip", "Assets/TemplateSDK/Templates/" + ValidName2 + ".template");
        AssetDatabase.ImportAsset("Assets/TemplateSDK/Templates/" + ValidName2 + ".template");
        Debug.Log("<color=lime>AVATAR TEMPLATE COMPLETE!</color>");
    }

    /*IEnumerator PrepareWebPackage()
    {
        
    }*/

    //[MenuItem("Avatar Template/(2) Build Templates")]
    public void BuildAllAssetBundles(BuildTarget BuildTo)
    {
        string assetBundleDirectory = "Assets/TemplateSDK/Templates/Temp";
        //Debug.Log("Building Templates");
        Directory.CreateDirectory(assetBundleDirectory);
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, BuildTo);
        AssetDatabase.Refresh();
        Debug.Log("<color=green>"+ BuildTo.ToString() +" Compiled!</color>");
        //purge temp / modified assets
        AssetDatabase.DeleteAsset("Assets/TemplateSDK/Templates/Temp/Avatar.prefab");
        if (BuildTo == BuildTarget.WebGL) {
            VRCAvatarDescriptor Avi = AvatarGO.AddComponent<VRCAvatarDescriptor>();
            PipelineManager Pipe = AvatarGO.AddComponent<PipelineManager>();

            Avi.GetCopyOf(TempDesc);
            Pipe.GetCopyOf(TempManager);
            AvatarCustomizationOptions.Avatar = Avi;
            DestroyImmediate(TempGO);
        }
    }
}

//for webgl vrc component preservation
public static class CopyComponent
{
    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

    public static void CompressDirectory(string sourcePath, string archivePath, bool overwrite = true)
    {
        if (overwrite)
        {
            File.Delete(archivePath);
        }

        ZipFile.CreateFromDirectory(sourcePath, archivePath);
    }
}