#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;

[System.Serializable]
public class TagMask
{
    [SerializeField, HideInInspector] private int mask;

    /// <summary>Returns true if the given tag is selected in the mask.</summary>
    public bool Contains(string tag)
    {
        int index = GetTagIndex(tag);
        return index >= 0 && (mask & (1 << index)) != 0;
    }

    /// <summary>Build a tag mask from selected tag names.</summary>
    public void SetSelectedTags(string[] selectedTags)
    {
        mask = 0;
        foreach (var tag in selectedTags)
        {
            int index = GetTagIndex(tag);
            if (index >= 0)
                mask |= 1 << index;
        }
    }

    /// <summary>Returns all selected tag names.</summary>
    public string[] GetSelectedTags()
    {
        var allTags = UnityEditorInternal.InternalEditorUtility.tags;
        var result = new System.Collections.Generic.List<string>();
        for (int i = 0; i < allTags.Length; i++)
        {
            if ((mask & (1 << i)) != 0)
                result.Add(allTags[i]);
        }
        return result.ToArray();
    }

    private int GetTagIndex(string tag)
    {
        var allTags = UnityEditorInternal.InternalEditorUtility.tags;
        for (int i = 0; i < allTags.Length; i++)
        {
            if (allTags[i] == tag) return i;
        }
        return -1;
    }

    public int RawMask => mask;
    public void SetRawMask(int raw) => mask = raw;
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TagMask))]
public class TagMaskDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rawMaskProp = property.FindPropertyRelative("mask");

        string[] tags = InternalEditorUtility.tags;
        int currentMask = rawMaskProp.intValue;

        int newMask = EditorGUI.MaskField(position, label, currentMask, tags);
        if (newMask != currentMask)
            rawMaskProp.intValue = newMask;
    }
}
#endif