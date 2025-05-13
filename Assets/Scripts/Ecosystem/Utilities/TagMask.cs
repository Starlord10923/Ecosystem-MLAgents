using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[Serializable]
public class TagMask
{
    [SerializeField]
    private List<string> selectedTags = new List<string>();

    private HashSet<string> _tagSet;

    public bool Contains(string tag)
    {
        EnsureCache();
        return _tagSet.Contains(tag);
    }

    public string[] GetSelectedTags()
    {
        return selectedTags.ToArray();
    }

    public void SetSelectedTags(string[] tags)
    {
        selectedTags = new List<string>(tags);
        _tagSet = null; // reset cache
    }

    private void EnsureCache()
    {
        _tagSet ??= new HashSet<string>(selectedTags);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(TagMask))]
public class TagMaskDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var tagListProp = property.FindPropertyRelative("selectedTags");
        string[] allTags = InternalEditorUtility.tags;

        int currentMask = 0;
        for (int i = 0; i < allTags.Length; i++)
        {
            if (tagListProp.Contains(allTags[i]))
                currentMask |= 1 << i;
        }

        int newMask = EditorGUI.MaskField(position, label, currentMask, allTags);
        if (newMask != currentMask)
        {
            tagListProp.ClearArray();
            for (int i = 0; i < allTags.Length; i++)
            {
                if ((newMask & (1 << i)) != 0)
                {
                    int index = tagListProp.arraySize;
                    tagListProp.InsertArrayElementAtIndex(index);
                    tagListProp.GetArrayElementAtIndex(index).stringValue = allTags[i];
                }
            }
        }
    }
}

public static class SerializedPropertyExtensions
{
    public static bool Contains(this SerializedProperty list, string value)
    {
        for (int i = 0; i < list.arraySize; i++)
        {
            if (list.GetArrayElementAtIndex(i).stringValue == value)
                return true;
        }
        return false;
    }
}
#endif