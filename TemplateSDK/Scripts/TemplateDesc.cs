using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

public class TemplateDesc : MonoBehaviour
{
    [HideInInspector] public Customization Template;
    [HideInInspector] public Vector3 ViewPos;
    [Serializable]
    public struct Customization 
    {
        [Header("Avatar Info:")]
        public VRCAvatarDescriptor Avatar;
        public string Name;
        [Header("Body Customization:")]
        public ItemCategory Head;
        public ItemCategory Torso, LeftArm, RightArm, LeftLeg, RightLeg;
        [Header("Other:")]
        public MiscItem[] Misc;
        //public AnimationClip WebIdleAnim;
    }
    [Serializable]
    public struct ItemCategory
    {
        public bool CanBeNothing, CanBeMultiple;
        public SkinnedItem[] Items;
    }

    [Serializable]
    public struct SkinnedItem
    {
        public string Name;
        public Sprite Icon;
        public MeshDesc[] Meshes;
        [HideInInspector] public bool Using;
    }

    [Serializable]
    public struct MeshDesc
    {
        public SkinnedMeshRenderer Mesh;
        [Header("Optional:")]
        public string VariantName;
        public MatDesc[] MaterialVariants;
    }
    [Serializable]
    public struct MatDesc
    {
        public string Name;
        public Sprite Icon;
        public Material Variant;
        [HideInInspector] public bool Using;
    }
    [Serializable]
    public struct MiscItem
    {
        public string Name;
        public Sprite Icon;
        public GameObject Object;
        [HideInInspector] public bool Using;
    }
}
