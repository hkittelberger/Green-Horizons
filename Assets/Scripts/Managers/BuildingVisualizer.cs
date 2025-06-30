using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(BuildingVisualizer))]
public class BuildingVisualizerEditor : Editor
{
    private int selectedTab = 0;
    private readonly string[] tabLabels = { "Buildings"};

    public override void OnInspectorGUI()
    {
        BuildingVisualizer manager = (BuildingVisualizer)target;

        EditorGUILayout.Space();
        selectedTab = GUILayout.Toolbar(selectedTab, tabLabels);
        EditorGUILayout.Space();

        serializedObject.Update();

        switch (selectedTab)
        {
            case 0: // Buildings tab
                DrawPrefabSection("Building Prefabs", serializedObject.FindProperty("buildingPrefabs"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPrefabSection(string sectionTitle, SerializedProperty property)
    {
        EditorGUILayout.LabelField(sectionTitle, EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        if (property != null)
        {
            EditorGUILayout.PropertyField(property, true);
        }
        else
        {
            EditorGUILayout.HelpBox($"Property not found for {sectionTitle}", MessageType.Warning);
        }
    }
}
#endif

public enum BuildingType
{
    City,
    Settlement,
    SolarPanel,
    PowerPlant,
    Farm,
    HurricaneShield
}

[Serializable]
public class BuildingPrefabEntry
{
    public BuildingType type;
    public Transform prefab;
}

public class BuildingVisualizer : NetworkBehaviour
{
    public static BuildingVisualizer Instance { get; private set; }

    [SerializeField] private List<BuildingPrefabEntry> buildingPrefabs = new();
    private readonly Dictionary<BuildingType, Transform> buildingPrefabDict = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializePrefabDictionary();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnClickedOnBuild += OnClickedOnBuild;
            }
            else
            {
                Debug.LogWarning("GameManager instance not ready. BuildingVisualizer will not receive clicks.");
            }
        }
    }

    private void InitializePrefabDictionary()
    {
        buildingPrefabDict.Clear();

        foreach (var entry in buildingPrefabs)
        {
            if (!buildingPrefabDict.ContainsKey(entry.type) && entry.prefab != null)
            {
                buildingPrefabDict[entry.type] = entry.prefab;
            }
            else if (entry.prefab == null)
            {
                Debug.LogWarning($"Missing prefab for {entry.type}");
            }
        }
    }

    private void OnClickedOnBuild(object sender, GameManager.OnClickedOnBuildEventArgs e)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the server should spawn buildings.");
            return;
        }

        if (TryGetPrefab(e.BuildTypeEnum, out var prefab))
        {
            Vector3 adjustedPosition = e.Tile.transform.position + new Vector3(0, 0, -0.1f);
            Transform spawned = Instantiate(prefab, adjustedPosition, Quaternion.identity);

            if (spawned.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn(destroyWithScene: true);
            }
            else
            {
                Debug.LogError($"Prefab for {e.BuildTypeEnum} is missing NetworkObject.");
            }
        }
        else
        {
            Debug.LogError($"No prefab found for building type: {e.BuildTypeEnum}");
        }
    }

    public bool TryGetPrefab(BuildingType type, out Transform prefab)
    {
        return buildingPrefabDict.TryGetValue(type, out prefab);
    }

    // Optional utility to auto-fill all enum entries
    [ContextMenu("Initialize All Building Types")]
    private void FillMissingBuildingTypes()
    {
        var existingTypes = new HashSet<BuildingType>();
        foreach (var entry in buildingPrefabs)
            existingTypes.Add(entry.type);

        foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
        {
            if (!existingTypes.Contains(type))
                buildingPrefabs.Add(new BuildingPrefabEntry { type = type });
        }
    }
}