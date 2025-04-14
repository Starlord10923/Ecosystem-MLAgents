using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ArenaSpawner : MonoBehaviour
{
    public List<GameObject> arenaPrefabs;
    public int rows = 8;
    public int cols = 8;
    public float spacing = 12f;

    [ContextMenu("Spawn Multiple Environments")]
    void Spawn()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

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
    }
}
