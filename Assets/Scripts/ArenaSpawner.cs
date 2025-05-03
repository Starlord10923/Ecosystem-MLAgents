using System.Collections.Generic;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEngine;

public class ArenaSpawner : MonoBehaviour
{
    public List<GameObject> arenaPrefabs;
    public int rows = 8;
    public int cols = 8;
    public float spacing = 12f;
    [Tooltip("Optional seed for prefab selection. Leave -1 for random.")]
    public int seed = -1;

    public NNModel brain;
    public bool useBrain = false;

    [ContextMenu("Spawn Multiple Environments")]
    void Spawn()
    {
        if (arenaPrefabs == null || arenaPrefabs.Count == 0)
        {
            Debug.LogError("ArenaSpawner: No arena prefabs assigned!");
            return;
        }

        // Clear existing children
        ClearArenas();

        if (seed >= 0) Random.InitState(seed);

        // Spawn grid of arenas
        for (int x = 0; x < cols; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 pos = new Vector3(x * spacing, 0, z * spacing);
                GameObject arenaPrefab = arenaPrefabs[Random.Range(0, arenaPrefabs.Count)];
                GameObject spawned = (GameObject)PrefabUtility.InstantiatePrefab(arenaPrefab, transform);
                spawned.name = $"{arenaPrefab.name} ({x * cols + z})";
                spawned.transform.localPosition = pos;
            }
        }
        ApplyBrainSettings();
    }

    private void SetBrainParams(BehaviorParameters behaviorParams)
    {
        if (useBrain && brain != null)
        {
            behaviorParams.BehaviorType = BehaviorType.InferenceOnly;
            behaviorParams.BehaviorName = brain.name;
            behaviorParams.Model = brain;
            Debug.Log($"Set {behaviorParams.gameObject.name} to Inference using brain.");
        }
        else
        {
            behaviorParams.BehaviorType = BehaviorType.HeuristicOnly;
            behaviorParams.Model = null;
        }
    }

    [ContextMenu("Clear Spawned Arenas")]
    void ClearArenas()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    [ContextMenu("Apply Brain Settings To All")]
    public void ApplyBrainSettings()
    {
        foreach (var behaviorParams in GetComponentsInChildren<BehaviorParameters>())
        {
            SetBrainParams(behaviorParams);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Handles.color = Color.cyan;
        for (int x = 0; x < cols; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 pos = transform.position + new Vector3(x * spacing, 0, z * spacing);
                Handles.Label(pos + Vector3.up * 1.5f, $"({x},{z})");
            }
        }
    }
#endif

}
