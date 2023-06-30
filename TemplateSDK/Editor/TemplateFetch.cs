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
using static TemplateDesc;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;

public class TemplateFetch : EditorWindow
{
    public List<Template> TemplatesFixed = new List<Template>();

    public string Templates;

    Vector2 scrollPos;

    bool Loading;

    [Serializable]
    public struct Template
    {
        public string Name;
        public string Author;
        [HideInInspector] public string Url;
    }

    [MenuItem("Avatar Template/(tools)/Import Template")]
    public static void ShowWindow()
    {
        TemplateFetch Fetcher = EditorWindow.GetWindow(typeof(TemplateFetch)) as TemplateFetch;
        EditorCoroutineUtility.StartCoroutine(Fetcher.RetrieveTemplates(), Fetcher);
    }

    private void OnGUI()
    {
        //Scrolling
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("Select a template to import to this project.");

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (Loading) {
            GUILayout.Label("Loading content... Please wait.");
        }
        else {
            foreach (Template temp in TemplatesFixed)
            {
                if (GUILayout.Button(temp.Name + " | By: " + temp.Author))
                {
                    EditorCoroutineUtility.StartCoroutine(FetchTemplate(temp.Url), this);
                }
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        EditorGUILayout.EndScrollView();
    }

    public IEnumerator FetchTemplate(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get("http://10.0.0.206/content/avatarTemplates/" + url + "/standalonewindows.template");

        var loader = new WWW("http://10.0.0.206/content/avatarTemplates/" + url + "/standalonewindows.template");
        //"http://swagstools.com/content/avatarTemplates/" + url + "/standalonewindows.template;

        yield return loader;

        var myLoadedAssetBundle = loader.assetBundle;
        var prefab = myLoadedAssetBundle.LoadAsset<GameObject>("Avatar");
        GameObject Go = Instantiate(prefab);

        TemplateBuilder Builder = EditorWindow.GetWindow(typeof(TemplateBuilder)) as TemplateBuilder;
        Builder.AvatarCustomizationOptions = Go.transform.GetComponentsInChildren<TemplateDesc>()[0].Template;

        AssetDatabase.Refresh();
    }

        public IEnumerator RetrieveTemplates()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://10.0.0.206/pages/avatarTemplates.php?");

        //UnityWebRequest www = UnityWebRequest.Get("http://swagstools.com/pages/avatarTemplates.php?");

        TemplatesFixed.Clear();

        Loading = true;
        yield return www.SendWebRequest();

        if (www.isDone)
        {
            Loading = false;
            Templates = www.downloadHandler.text;
            //Debug.Log(Templates);
            //Isolates results to just template data
            string[] TemplatesIsolated = splitString("TEMPLATEDATA", Templates);//Templates.Split(char.Parse("TEMPLATEDATA"));
                                                                                //Splits each template from one another into seperate strings
            string[] TemplatesSplit = splitString("<br>", TemplatesIsolated[1]);//Templates.Split(char.Parse("<br>"));
                                                                                //parses individual template data from strings

            for (int i = 0; i < TemplatesSplit.Length - 1; i++)
            {
                string[] Details = TemplatesSplit[i].Split('▞');
                //Debug.Log("Template: " + Details[1] + " by: " + Details[0]);
                Template temp = new Template();
                temp.Name = Details[1];
                temp.Author = Details[0];
                temp.Url = TemplatesSplit[i];

                Debug.Log(temp.Name + " = " + temp.Url);

                TemplatesFixed.Add(temp);
            }
        }
        else
        {
            Loading = false;
            Debug.Log("FAILED");
        }
    }

    public string[] splitString(string needle, string haystack)
    {
        //This will look for NEEDLE in HAYSTACK and return an array of split strings.
        //NOTE: If the returned array has a length of 1 (meaning it only contains
        //        element [0]) then that means NEEDLE was NOT found.

        return haystack.Split(new string[] { needle }, System.StringSplitOptions.None);

    }
}