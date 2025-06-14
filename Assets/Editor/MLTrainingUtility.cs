// File: Assets/Editor/MLTrainingUtility.cs
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

public class MLTrainingUtility : EditorWindow
{
    // --- Pref keys ---
    private const string PREF_CONDA_ENV           = "MLUtil_CondaEnv";
    private const string PREF_CONFIG_PATH         = "MLUtil_ConfigPath";
    private const string PREF_EXE_PATH            = "MLUtil_ExePath";
    private const string PREF_NUM_ENVS            = "MLUtil_NumEnvs";
    private const string PREF_TIME_SCALE          = "MLUtil_TimeScale";
    private const string PREF_BASE_PORT           = "MLUtil_BasePort";
    private const string PREF_TB_PORT             = "MLUtil_TensorboardPort";
    private const string PREF_USE_GRAPHICS        = "MLUtil_UseGraphics";
    private const string PREF_OPEN_VSCODE         = "MLUtil_OpenVSCode";
    private const string PREF_USE_TENSORBOARD     = "MLUtil_UseTensorboard";
    private const string PREF_TRAIN_IN_EDITOR     = "MLUtil_TrainInEditor";
    private const string PREF_USE_EXISTING_RUN_ID = "MLUtil_UseExistingRunId";

    // --- UI state ---
    private string  condaEnvName;
    private string  configPath;
    private string  exePath;
    private int     numEnvs;
    private float   timeScale;
    private string  basePort;
    private string  tensorboardPort;
    private bool    useGraphics;
    private bool    openVSCode;
    private bool    useTensorBoard;
    private bool    trainInEditor;
    private bool    useExistingRunId;

    private string[] runIds = new string[0];
    private int      runIndex;
    private string   newRunId = "MyRun01";

    // --- Config‐tweak state ---
    private int  cfg_MaxSteps;
    private int  cfg_CheckpointInterval;
    private int  cfg_SummaryFreq;
    private int  cfg_KeepCheckpoints;
    private bool cfg_Dirty;

    // --- Advanced settings UI & state ---
    private bool   showAdvancedSettings;
    private int    advTrainerTypeIndex;
    private string[] advTrainerTypes = { "ppo", "sac" };
    private int    adv_HiddenUnits;
    private int    adv_NumLayers;
    private float  adv_LearningRate;
    private float  adv_Gamma;

    // Track last loaded Run ID to reload config once per selection
    private string lastLoadedRunId;

    [MenuItem("Tools/ML Training Utility")]
    public static void ShowWindow()
    {
        var w = GetWindow<MLTrainingUtility>("ML Training Utility");
        w.minSize = new Vector2(500, 620);
        w.LoadPrefs();
        w.RefreshRunIds();
    }

    private void OnGUI()
    {
        GUILayout.Space(8);
        DrawCondaSection();
        GUILayout.Space(8);
        DrawConfigFileSection();
        GUILayout.Space(12);
        DrawTrainingModeSection();
        GUILayout.Space(12);
        DrawTrainingSetupSection();
        GUILayout.Space(12);
        DrawRunIdSection();
        GUILayout.Space(12);
        DrawConfigTweaksSection();
        GUILayout.Space(12);
        DrawUtilitiesSection();

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("▶ Start Training", GUILayout.Height(36)))
            StartTraining();

        if (GUI.changed)
            SavePrefs();
    }

    private void DrawCondaSection()
    {
        EditorGUILayout.LabelField("❐ Conda Environment", EditorStyles.boldLabel);
        condaEnvName = EditorGUILayout.TextField("Env Name", condaEnvName);
    }

    private void DrawConfigFileSection()
    {
        EditorGUILayout.LabelField("❐ Configuration File", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        configPath = EditorGUILayout.TextField(configPath);
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(70)))
            PickConfigFile();
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Generate Default Config Template"))
            GenerateConfigTemplate();
    }

    private void DrawTrainingModeSection()
    {
        EditorGUILayout.LabelField("❐ Training Mode", EditorStyles.boldLabel);
        trainInEditor = EditorGUILayout.Toggle(
            new GUIContent("Train in Editor", "Runs in PlayMode instead of a build EXE"),
            trainInEditor);
    }

    private void DrawTrainingSetupSection()
    {
        EditorGUILayout.LabelField("❐ Training Setup", EditorStyles.boldLabel);

        if (!trainInEditor)
        {
            useGraphics = EditorGUILayout.Toggle(
                new GUIContent("Run with Graphics", "Uncheck to pass --no-graphics"),
                useGraphics);

            EditorGUILayout.BeginHorizontal();
            exePath = EditorGUILayout.TextField("Build EXE Path", exePath);
            if (GUILayout.Button("Browse", GUILayout.MaxWidth(70)))
                PickExeFile();
            EditorGUILayout.EndHorizontal();

            numEnvs = EditorGUILayout.IntField(
                new GUIContent("Num Envs", "Passes --num-envs"), numEnvs);
        }

        timeScale = EditorGUILayout.FloatField(
            new GUIContent("Time Scale", "Passes --time-scale"), timeScale);

        basePort = EditorGUILayout.TextField(
            new GUIContent("Base Port", "Passes --base-port"), basePort);
    }

    private void DrawRunIdSection()
    {
        EditorGUILayout.LabelField("❐ Run ID", EditorStyles.boldLabel);
        useExistingRunId = EditorGUILayout.Toggle(
            "Use Existing Run ID", useExistingRunId);

        if (useExistingRunId)
        {
            if (runIds.Length > 0)
            {
                var prevIndex = runIndex;
                runIndex = EditorGUILayout.Popup("Existing IDs", runIndex, runIds);
                newRunId = runIds[runIndex];
                if (runIndex != prevIndex)
                    lastLoadedRunId = null;  // force reload
            }
            else
            {
                EditorGUILayout.HelpBox("No run folders found under results/", MessageType.Info);
            }
        }
        else
        {
            newRunId = EditorGUILayout.TextField("New Run ID", newRunId);
        }

        if (GUILayout.Button("Refresh Run IDs", GUILayout.MaxWidth(130)))
            RefreshRunIds();
    }

    private void DrawConfigTweaksSection()
    {
        if (useExistingRunId && newRunId != lastLoadedRunId && !cfg_Dirty)
        {
            LoadConfigSettings();
            lastLoadedRunId = newRunId;
        }

        EditorGUILayout.LabelField("❐ Config Tweaks", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        cfg_MaxSteps           = EditorGUILayout.IntField("Max Steps",           cfg_MaxSteps);
        cfg_CheckpointInterval = EditorGUILayout.IntField("Checkpoint Interval", cfg_CheckpointInterval);
        cfg_SummaryFreq        = EditorGUILayout.IntField("Summary Frequency",   cfg_SummaryFreq);
        cfg_KeepCheckpoints    = EditorGUILayout.IntField("Keep Checkpoints",    cfg_KeepCheckpoints);
        if (EditorGUI.EndChangeCheck())
            cfg_Dirty = true;

        GUILayout.Space(4);
        showAdvancedSettings = EditorGUILayout.Foldout(
            showAdvancedSettings, "Advanced Settings", true);
        if (showAdvancedSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            advTrainerTypeIndex = EditorGUILayout.Popup(
                "Trainer Type", advTrainerTypeIndex, advTrainerTypes);
            adv_HiddenUnits     = EditorGUILayout.IntField(
                "Hidden Units", adv_HiddenUnits);
            adv_NumLayers       = EditorGUILayout.IntField(
                "Num Layers", adv_NumLayers);
            adv_LearningRate    = EditorGUILayout.FloatField(
                "Learning Rate", adv_LearningRate);
            adv_Gamma           = EditorGUILayout.FloatField(
                "Gamma (Discount)", adv_Gamma);
            if (EditorGUI.EndChangeCheck())
                cfg_Dirty = true;
            EditorGUI.indentLevel--;
        }
    }

    private void DrawUtilitiesSection()
    {
        EditorGUILayout.LabelField("❐ Utilities", EditorStyles.boldLabel);
        openVSCode     = EditorGUILayout.Toggle("Open VS Code",       openVSCode);
        useTensorBoard = EditorGUILayout.Toggle("Launch TensorBoard", useTensorBoard);
        if (useTensorBoard)
            tensorboardPort = EditorGUILayout.TextField("TB Port", tensorboardPort);
    }

    private void LoadPrefs()
    {
        condaEnvName     = EditorPrefs.GetString(PREF_CONDA_ENV,           "UnityML");
        configPath       = EditorPrefs.GetString(PREF_CONFIG_PATH,         "Assets/ML-Agents/configuration.yaml");
        exePath          = EditorPrefs.GetString(PREF_EXE_PATH,            @"D:/Unity Projects/Ecosystem-MLAgents/Builds/Ecosystem-MLAgents.exe");
        numEnvs          = EditorPrefs.GetInt(PREF_NUM_ENVS,               2);
        timeScale        = EditorPrefs.GetFloat(PREF_TIME_SCALE,           1f);
        basePort         = EditorPrefs.GetString(PREF_BASE_PORT,           "5005");
        tensorboardPort  = EditorPrefs.GetString(PREF_TB_PORT,             "6006");
        useGraphics      = EditorPrefs.GetBool(PREF_USE_GRAPHICS,          true);
        openVSCode       = EditorPrefs.GetBool(PREF_OPEN_VSCODE,           true);
        useTensorBoard   = EditorPrefs.GetBool(PREF_USE_TENSORBOARD,       false);
        trainInEditor    = EditorPrefs.GetBool(PREF_TRAIN_IN_EDITOR,       false);
        useExistingRunId = EditorPrefs.GetBool(PREF_USE_EXISTING_RUN_ID,   false);

        // Default advanced to match typical config
        advTrainerTypeIndex = 0;
        adv_HiddenUnits     = 128;
        adv_NumLayers       = 2;
        adv_LearningRate    = 0.0003f;
        adv_Gamma           = 0.99f;
    }

    private void SavePrefs()
    {
        EditorPrefs.SetString(PREF_CONDA_ENV,           condaEnvName);
        EditorPrefs.SetString(PREF_CONFIG_PATH,         configPath);
        EditorPrefs.SetString(PREF_EXE_PATH,            exePath);
        EditorPrefs.SetInt(PREF_NUM_ENVS,               numEnvs);
        EditorPrefs.SetFloat(PREF_TIME_SCALE,           timeScale);
        EditorPrefs.SetString(PREF_BASE_PORT,           basePort);
        EditorPrefs.SetString(PREF_TB_PORT,             tensorboardPort);
        EditorPrefs.SetBool(PREF_USE_GRAPHICS,          useGraphics);
        EditorPrefs.SetBool(PREF_OPEN_VSCODE,           openVSCode);
        EditorPrefs.SetBool(PREF_USE_TENSORBOARD,       useTensorBoard);
        EditorPrefs.SetBool(PREF_TRAIN_IN_EDITOR,       trainInEditor);
        EditorPrefs.SetBool(PREF_USE_EXISTING_RUN_ID,   useExistingRunId);
    }

    private void PickConfigFile()
    {
        var path = EditorUtility.OpenFilePanel("Select YAML Config", Application.dataPath, "yaml");
        if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            configPath = "Assets" + path.Substring(Application.dataPath.Length);
    }

    private void PickExeFile()
    {
        var path = EditorUtility.OpenFilePanel("Select Build Executable", "", "exe");
        if (!string.IsNullOrEmpty(path))
            exePath = path;
    }

    private void RefreshRunIds()
    {
        var root    = Path.Combine(Application.dataPath, "..");
        var results = Path.Combine(root, "results");
        if (Directory.Exists(results))
        {
            var dirs = Directory.GetDirectories(results);
            runIds = System.Array.ConvertAll(dirs, d => Path.GetFileName(d));
        }
        else runIds = new string[0];
        runIndex = 0;
    }

    private void GenerateConfigTemplate()
    {
        var assetPath = Path.Combine(Application.dataPath, "ML-Agents/DefaultConfig.yaml");
        var fullPath  = Path.Combine(Application.dataPath, configPath.Substring(7));

        if (!File.Exists(fullPath))
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        if (File.Exists(assetPath))
        {
            File.Copy(assetPath, fullPath, true);
        }
        else
        {
            const string yaml = @"
behaviors:
  MyBehavior:
    trainer_type: ppo
    hyperparameters:
      batch_size: 1024
      buffer_size: 20480
    network_settings:
      normalize: false
      hidden_units: 256
      num_layers: 2
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
    keep_checkpoints: 5
    max_steps: 500000
    time_horizon: 64
    summary_freq: 10000
";
            File.WriteAllText(fullPath, yaml.TrimStart('\n'));
        }

        AssetDatabase.Refresh();
        Debug.Log($"[MLUtil] Default config written to {configPath}");
    }

    private void LoadConfigSettings()
    {
        cfg_Dirty = false;
        var relPath = configPath.StartsWith("Assets/") 
            ? configPath.Substring(7) 
            : configPath;
        var full    = Path.Combine(Application.dataPath, relPath);
        if (!File.Exists(full)) return;

        var lines = File.ReadAllLines(full);
        foreach (var line in lines)
        {
            var t = line.TrimStart();
            if (t.StartsWith("checkpoint_interval:"))
                cfg_CheckpointInterval = int.Parse(t.Split(':')[1]);
            if (t.StartsWith("max_steps:"))
                cfg_MaxSteps = int.Parse(t.Split(':')[1]);
            if (t.StartsWith("summary_freq:"))
                cfg_SummaryFreq = int.Parse(t.Split(':')[1]);
            if (t.StartsWith("keep_checkpoints:"))
                cfg_KeepCheckpoints = int.Parse(t.Split(':')[1]);
            if (t.StartsWith("trainer_type:"))
                advTrainerTypeIndex = System.Array.IndexOf(
                    advTrainerTypes, t.Split(':')[1].Trim());
            if (t.StartsWith("hidden_units:"))
                adv_HiddenUnits = int.Parse(t.Split(':')[1]);
            if (t.StartsWith("num_layers:"))
                adv_NumLayers = int.Parse(t.Split(':')[1]);
            if (t.StartsWith("learning_rate:"))
                adv_LearningRate = float.Parse(t.Split(':')[1]);
            if (t.StartsWith("gamma:"))
                adv_Gamma = float.Parse(t.Split(':')[1]);
        }
    }

    private void SaveConfigSettingsToFile()
    {
        if (!cfg_Dirty) return;

        var relPath = configPath.StartsWith("Assets/") 
            ? configPath.Substring(7) 
            : configPath;
        var full    = Path.Combine(Application.dataPath, relPath);
        if (!File.Exists(full)) return;

        var lines = File.ReadAllLines(full);
        bool replacedHidden = false, replacedLayers = false;
        for (int i = 0; i < lines.Length; i++)
        {
            var t = lines[i].TrimStart();
            if (t.StartsWith("checkpoint_interval:"))
                lines[i] = lines[i].ReplaceAfter(
                    "checkpoint_interval:", $" {cfg_CheckpointInterval}");
            else if (t.StartsWith("max_steps:"))
                lines[i] = lines[i].ReplaceAfter(
                    "max_steps:", $" {cfg_MaxSteps}");
            else if (t.StartsWith("summary_freq:"))
                lines[i] = lines[i].ReplaceAfter(
                    "summary_freq:", $" {cfg_SummaryFreq}");
            else if (t.StartsWith("keep_checkpoints:"))
                lines[i] = lines[i].ReplaceAfter(
                    "keep_checkpoints:", $" {cfg_KeepCheckpoints}");
            else if (t.StartsWith("trainer_type:"))
                lines[i] = lines[i].ReplaceAfter(
                    "trainer_type:", $" {advTrainerTypes[advTrainerTypeIndex]}");
            else if (!replacedHidden && t.StartsWith("hidden_units:"))
            {
                lines[i] = lines[i].ReplaceAfter(
                    "hidden_units:", $" {adv_HiddenUnits}");
                replacedHidden = true;
            }
            else if (!replacedLayers && t.StartsWith("num_layers:"))
            {
                lines[i] = lines[i].ReplaceAfter(
                    "num_layers:", $" {adv_NumLayers}");
                replacedLayers = true;
            }
            else if (t.StartsWith("learning_rate:"))
                lines[i] = lines[i].ReplaceAfter(
                    "learning_rate:", $" {adv_LearningRate}");
            else if (t.StartsWith("gamma:"))
                lines[i] = lines[i].ReplaceAfter(
                    "gamma:", $" {adv_Gamma}");
        }

        File.WriteAllLines(full, lines);
        cfg_Dirty = false;
    }

    private async void StartTraining()
    {
        // Persist any YAML tweaks
        SaveConfigSettingsToFile();

        var projectRoot = Path.Combine(Application.dataPath, "..");

        // Build the core mlagents-learn call
        var cmdArgs = $"mlagents-learn \"{configPath}\" --run-id=\"{newRunId}\" --force";
        if (!trainInEditor)
        {
            var gfxArg = useGraphics ? "" : "--no-graphics";
            cmdArgs += $" --env=\"{exePath}\" {gfxArg} --num-envs={numEnvs}";
        }
        cmdArgs += $" --time-scale={timeScale} --base-port={basePort}";

        // Full shell command
        var fullCmd    = $"conda activate {condaEnvName} && {cmdArgs}";
        var jsonEscaped = $"{{\"text\":\"{fullCmd}\\u000D\"}}";

        if (openVSCode)
        {
            // 1) Focus or open the workspace in the existing window
            Process.Start("code", $"--reuse-window \"{projectRoot}\"");
            await Task.Delay(2000);

            // 2) Create a new integrated terminal
            Process.Start("code", "--command workbench.action.terminal.new");
            await Task.Delay(500);

            // 3) Send the training command + Enter
            Process.Start("code",
                $"--command workbench.action.terminal.sendSequence --args \"{jsonEscaped}\"");
        }
        else
        {
            // Fallback: external CMD
            Process.Start("cmd.exe", $"/k {fullCmd}");
        }

        Debug.Log($"[MLUtil] Dispatched to VS Code terminal:\n  {fullCmd}");
    }
}

// Helper extension for in-place YAML edits
static class StringExtensions
{
    public static string ReplaceAfter(this string line, string key, string suffix)
    {
        var idx = line.IndexOf(key) + key.Length;
        return line.Substring(0, idx) + suffix;
    }
}
