using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
#if ADDRESSABLES_ENABLED
    using UnityEngine.AddressableAssets;
#endif

namespace DarkTonic.PoolBoss.EditorScript
{
    // ReSharper disable once CheckNamespace
    public static class DTPoolBossInspectorUtility
    {
        private const string FoldOutTooltip = "Click to expand or collapse";
        private const string AlertTitle = "Pool Boss Alert";
        private const string AlertOkText = "Ok";
        private const string DownArrow = "\u25BC";


        // ReSharper disable InconsistentNaming
        // COLORS FOR DARK SCHEME
        private static readonly Color DarkSkin_DragAreaColor = Color.yellow;
        private static readonly Color DarkSkin_InactiveHeaderColor = new Color(.6f, .6f, .6f);
        private static readonly Color DarkSkin_ActiveHeaderColor = new Color(.3f, .8f, 1f);
        private static readonly Color DarkSkin_OuterGroupBoxColor = new Color(.7f, 1f, 1f);
        private static readonly Color DarkSkin_GroupBoxColor = new Color(.6f, .6f, .6f);
        private static readonly Color DarkSkin_SecondaryGroupBoxColor = new Color(.5f, .8f, 1f);
        private static readonly Color DarkSkin_SecondaryHeaderColor = new Color(.8f, .8f, .8f);
        private static readonly Color DarkSkin_BrightButtonColor = Color.cyan;
        private static readonly Color DarkSkin_BrightTextColor = Color.yellow;
        private static readonly Color DarkSkin_DeleteButtonColor = new Color(1f, .2f, .2f);
        private static readonly Color DarkSkin_AddButtonColor = Color.green;
        private static readonly Color DarkSkin_CloneButtonColor = new Color(1f, 1f, 1f);
        private static readonly Color DarkSkin_DividerColor = Color.gray;

        // COLORS FOR LIGHT SCHEME
        private static readonly Color LightSkin_OuterGroupBoxColor = Color.white;
        private static readonly Color LightSkin_DragAreaColor = new Color(1f, 1f, .3f);
        private static readonly Color LightSkin_InactiveHeaderColor = new Color(.6f, .6f, .6f);
        private static readonly Color LightSkin_ActiveHeaderColor = new Color(.3f, .8f, 1f);
        private static readonly Color LightSkin_GroupBoxColor = new Color(.7f, .7f, .8f);
        private static readonly Color LightSkin_SecondaryGroupBoxColor = new Color(.6f, 1f, 1f);
        private static readonly Color LightSkin_SecondaryHeaderColor = Color.white;
        private static readonly Color LightSkin_BrightButtonColor = new Color(0f, 1f, 1f);
        private static readonly Color LightSkin_BrightTextColor = Color.yellow;
        private static readonly Color LightSkin_DeleteButtonColor = new Color(1f, .2f, .2f);
        private static readonly Color LightSkin_AddButtonColor = Color.green;
        private static readonly Color LightSkin_CloneButtonColor = new Color(.3f, .6f, 1f);
        private static readonly Color LightSkin_DividerColor = new Color(.4f, .4f, .4f);
        // ReSharper restore InconsistentNaming

        public enum FunctionButtons { None, Add, Remove, ShiftUp, ShiftDown, Edit, DespawnAll, Rename, Cancel, Save, Copy }

        public static FunctionButtons AddFoldOutListItemButtons(int position, int totalPositions, string itemName, bool showFindButton, string findText, bool showAddButton, bool showMoveButtons = false, bool showCopyButton = false)
        {
            if (Application.isPlaying)
            {
                return FunctionButtons.None;
            }

            var oldBg = GUI.backgroundColor;
            var oldContent = GUI.contentColor;

            var shiftUp = false;
            var shiftDown = false;

            if (showMoveButtons)
            {
                if (position > 0)
                {
                    // the up arrow.
                    var upArrow = PoolBossInspectorResources.UpArrowTexture;
                    if (GUILayout.Button(new GUIContent(upArrow, "Click to shift " + itemName + " up"), EditorStyles.toolbarButton, GUILayout.Width(24)))
                    {
                        shiftUp = true;
                    }
                }
                else
                {
                    GUILayout.Space(24);
                }

                if (position < totalPositions - 1)
                {
                    // The down arrow will move things towards the end of the List
                    var dnArrow = PoolBossInspectorResources.DownArrowTexture;
                    if (GUILayout.Button(new GUIContent(dnArrow, "Click to shift " + itemName + " down"), EditorStyles.toolbarButton, GUILayout.Width(24)))
                    {
                        shiftDown = true;
                    }
                }
                else
                {
                    GUILayout.Space(24);
                }
            }

            var addPressed = false;

            if (showAddButton)
            {
                GUI.contentColor = AddButtonColor;

                addPressed = GUILayout.Button(new GUIContent("Add", "Click to insert " + itemName),
                                              EditorStyles.toolbarButton, GUILayout.Width(32));
            }
            GUI.contentColor = oldContent;

            var isCopy = false;

            if (showCopyButton)
            {
                isCopy = ShowCopyIcon() == FunctionButtons.Copy;
            }

            // Remove Button - Process presses later
            GUI.backgroundColor = DeleteButtonColor;
            if (GUILayout.Button(new GUIContent("Delete", "Click to remove " + itemName), EditorStyles.miniButton, GUILayout.Width(51)))
            {
                return FunctionButtons.Remove;
            }

            GUI.backgroundColor = oldBg;

            if (isCopy)
            {
                return FunctionButtons.Copy;
            }

            if (shiftUp)
            {
                return FunctionButtons.ShiftUp;
            }
            if (shiftDown)
            {
                return FunctionButtons.ShiftDown;
            }
            if (addPressed)
            {
                return FunctionButtons.Add;
            }

            return FunctionButtons.None;
        }

        public static void ResetColors()
        {
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.white;
        }

        public static Color AddButtonColor {
            get {
                return IsDarkSkin ? DarkSkin_AddButtonColor : LightSkin_AddButtonColor;
            }
        }

        public static Color BrightButtonColor {
            get {
                return IsDarkSkin ? DarkSkin_BrightButtonColor : LightSkin_BrightButtonColor;
            }
        }

        public static Color BrightTextColor {
            get {
                return IsDarkSkin ? DarkSkin_BrightTextColor : LightSkin_BrightTextColor;
            }
        }

        public static Color CloneButtonColor {
            get {
                return IsDarkSkin ? DarkSkin_CloneButtonColor : LightSkin_CloneButtonColor;
            }
        }

        public static Color DeleteButtonColor {
            get {
                return IsDarkSkin ? DarkSkin_DeleteButtonColor : LightSkin_DeleteButtonColor;
            }
        }

        private static Color GroupBoxColor {
            get {
                return IsDarkSkin ? DarkSkin_GroupBoxColor : LightSkin_GroupBoxColor;
            }
        }

        private static Color SecondaryGroupBoxColor {
            get {
                return IsDarkSkin ? DarkSkin_SecondaryGroupBoxColor : LightSkin_SecondaryGroupBoxColor;
            }
        }

        public static Color InactiveHeaderColor {
            get {
                return IsDarkSkin ? DarkSkin_InactiveHeaderColor : LightSkin_InactiveHeaderColor;
            }
        }

        public static Color ActiveHeaderColor {
            get {
                return IsDarkSkin ? DarkSkin_ActiveHeaderColor : LightSkin_ActiveHeaderColor;
            }
        }

        private static Color SecondaryHeaderColor {
            get {
                return IsDarkSkin ? DarkSkin_SecondaryHeaderColor : LightSkin_SecondaryHeaderColor;
            }
        }

        private static Color OuterGroupBoxColor {
            get {
                return IsDarkSkin ? DarkSkin_OuterGroupBoxColor : LightSkin_OuterGroupBoxColor;
            }
        }

        public static Color DragAreaColor {
            get {
                return IsDarkSkin ? DarkSkin_DragAreaColor : LightSkin_DragAreaColor;
            }
        }

        public static bool IsDarkSkin {
            get {
                return EditorPrefs.GetInt("UserSkin") == 1;
            }
        }

        public static Color DividerColor {
            get {
                return IsDarkSkin ? DarkSkin_DividerColor : LightSkin_DividerColor;
            }
        }

        public static void ShowCollapsibleSectionInline(ref bool state, string text)
        {
            var oldBG = GUI.backgroundColor;

            var style = new GUIStyle();
            style.fontSize = 11;
            style.fontStyle = FontStyle.Bold;
            style.margin = new RectOffset(3, 2, 0, 0);
            style.padding = new RectOffset(0, 0, 0, 0);
            style.fixedHeight = 17;


            GUILayout.BeginHorizontal(style);

            if (!state) {
                GUI.backgroundColor = InactiveHeaderColor;
            } else {
                GUI.backgroundColor = ActiveHeaderColor;
            }

            if (state)
            {
                text = DownArrow + " " + text;
            }
            else
            {
                text = "\u25BA " + text;
            }

            var headerStyle = new GUIStyle(EditorStyles.popup);
            headerStyle.fontSize = 11;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.margin = new RectOffset(0, 0, 0, 0);
            headerStyle.padding = new RectOffset(6, 0, 0, 0);
            headerStyle.fixedHeight = 22;

            if (!GUILayout.Toggle(true, text, headerStyle, GUILayout.MinWidth(20f)))
            {
                state = !state;
            }

            GUI.backgroundColor = oldBG;
        }

        public static void VerticalSpace(int pixels)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(pixels);
            EditorGUILayout.EndVertical();
        }

        public static void BeginGroupedControls()
        {
            GUI.backgroundColor = OuterGroupBoxColor;
            GUILayout.BeginHorizontal();

            EditorGUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(10f));

            GUILayout.BeginVertical();
            GUILayout.Space(2f);
            GUI.backgroundColor = Color.white;
        }

        public static void EndGroupedControls()
        {
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3f);
            GUILayout.EndHorizontal();

            GUILayout.Space(3f);
        }

        public static void StartGroupHeader(int level = 0, bool showBoth = true)
        {
            switch (level)
            {
                case 0:
                case 2:
                    GUI.backgroundColor = GroupBoxColor;
                    break;
                case 1:
                    GUI.backgroundColor = SecondaryGroupBoxColor;
                    break;
            }

            EditorGUILayout.BeginVertical(CornerGUIStyle);

            if (!showBoth)
            {
                GUI.backgroundColor = Color.white;
                return;
            }

            switch (level)
            {
                case 0:
                case 2:
                    GUI.backgroundColor = SecondaryHeaderColor;
                    break;
            }

            GUIStyle style = EditorStyles.objectFieldThumb;

            switch (level) {
                case 0:
                case 1:
                    break;
                case 2:
                    style = EditorStyles.objectField;
                    break;
            }

            GUIStyle textureStyle = new GUIStyle(style) {
                padding = new RectOffset(0, 3, 3, 4),
                margin = new RectOffset(0, 0, 0, 0)
            };
            EditorGUILayout.BeginVertical(textureStyle);

            GUI.backgroundColor = Color.white;
        }

        public static void EndGroupHeader()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private static GUIStyle CornerGUIStyle {
            get {
                return EditorStyles.helpBox;
            }

        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 2)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        public static bool Foldout(bool expanded, string label)
        {
            var content = new GUIContent(label, FoldOutTooltip);
            expanded = EditorGUILayout.Foldout(expanded, content);

            return expanded;
        }

        public static void DrawTexture(Texture tex)
        {
            if (tex == null)
            {
                Debug.Log("Logo texture missing");
                return;
            }

            var rect = GUILayoutUtility.GetRect(0f, 0f);
            rect.width = tex.width;
            rect.height = tex.height;
            GUILayout.Space(rect.height);
            GUI.DrawTexture(rect, tex);
        }

        public static void ShowColorWarning(string warningText)
        {
            EditorGUILayout.HelpBox(warningText, MessageType.Info);
        }

        public static void ShowRedError(string errorText)
        {
            EditorGUILayout.HelpBox(errorText, MessageType.Error);
        }

        public static void ShowLargeBarAlert(string errorText)
        {
            EditorGUILayout.HelpBox(errorText, MessageType.Warning);
        }

        public static void ShowAlert(string text)
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning(text);
            }
            else
            {
                EditorUtility.DisplayDialog(AlertTitle, text, AlertOkText);
            }
        }

        public static FunctionButtons AddCancelSaveButtons(string itemName)
        {
            var cancelIcon = new GUIContent(PoolBossInspectorResources.CancelTexture,
                                            "Click to cancel renaming " + itemName);

            GUI.backgroundColor = Color.white;
            if (GUILayout.Button(cancelIcon, EditorStyles.toolbarButton, GUILayout.Width(24),
                                 GUILayout.Height(16)))
            {
                return FunctionButtons.Cancel;
            }

            var saveIcon = new GUIContent(PoolBossInspectorResources.SaveTexture,
                                          "Click to save " + itemName);

            GUI.backgroundColor = Color.white;
            if (GUILayout.Button(saveIcon, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(16)))
            {
                return FunctionButtons.Save;
            }

            return FunctionButtons.None;
        }

        public static void FocusInProjectViewButton(string itemName, GameObject obj)
        {
            var settingsIcon = new GUIContent(PoolBossInspectorResources.PrefabTexture, "Click to select " + itemName + " in Project View");

            if (GUILayout.Button(settingsIcon, EditorStyles.toolbarButton, GUILayout.Width(24)))
            {
                EditorGUIUtility.PingObject(obj);
            }
        }

#if ADDRESSABLES_ENABLED
    public static void FocusAddressableInProjectViewButton(string itemName, AssetReference assetReference)
    {
        var settingsIcon = new GUIContent(PoolBossInspectorResources.PrefabTexture, "Click to select " + itemName + " in Project View");

        if (GUILayout.Button(settingsIcon, EditorStyles.toolbarButton, GUILayout.Width(24)))
        {
            if (!PoolBossAddressableEditorHelper.IsAddressableValid(assetReference))
            {
                return;
            }
            var path = AssetDatabase.GUIDToAssetPath(assetReference.RuntimeKey.ToString());
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            EditorGUIUtility.PingObject(obj);
        }
    }
#endif

        public static FunctionButtons ShowCopyIcon()
        {
            var oldColor = GUI.contentColor;

            GUI.contentColor = CloneButtonColor;

            var button = FunctionButtons.None;

            if (GUILayout.Button(new GUIContent(PoolBossInspectorResources.CopyTexture, "Click to clone item"), EditorStyles.miniButton, GUILayout.Height(16), GUILayout.Width(32)))
            {
                button = FunctionButtons.Copy;
            }

            GUI.contentColor = oldColor;

            return button;
        }

        public static bool IsInPrefabMode(GameObject gameObject)
        {
            return EditorSceneManager.IsPreviewScene(gameObject.scene);
        }
        public static void PrefabModeDoNotEdit()
        {
            ShowColorWarning("You cannot edit Pool Boss prefabs in prefab mode. Please return to Scene view.");
        }

        public static void MakePrefabMessage()
        {
            ShowRedError("Create your own prefab of this so it doesn't get overwritten the next time you update Pool Boss. Then this Inspector will become usable.");
        }

        public static bool IsPrefabInProjectView(GameObject gObject) {
            if (Application.isPlaying) {
                return false;
            }

            var isNonPrefab = GetPrefabType(gObject) == PrefabInstanceStatus.NotAPrefab;
        
            if (!isNonPrefab)
            {
                return false;
            }

            var isEmptyScene = gObject.scene.name;
            return string.IsNullOrEmpty(isEmptyScene);
        }

        private static PrefabInstanceStatus GetPrefabType(Object gObject) {
            return PrefabUtility.GetPrefabInstanceStatus(gObject);
        }
    }
}