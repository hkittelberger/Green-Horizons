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
public class PrefabEntry<T> where T : Enum
{
    [SerializeField] public T type;
    [SerializeField] public Transform prefab;

    public PrefabEntry(T type, Transform prefab = null)
    {
        this.type = type;
        this.prefab = prefab;
    }
}

[Serializable]
public class BuildingPrefabEntry : PrefabEntry<BuildingType>
{
    public BuildingPrefabEntry(BuildingType type, Transform prefab = null) : base(type, prefab) { }
}

public class BuildingVisualizer : MonoBehaviour
{
    public static BuildingVisualizer Instance { get; private set; }
    [SerializeField] private List<BuildingPrefabEntry> buildingPrefabs = new List<BuildingPrefabEntry>();

    // Dictionary for fast lookup during runtime
    private Dictionary<BuildingType, Transform> buildingPrefabDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePrefabDictionaries();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GameManager.Instance.OnClickedOnBuild += GameManager_OnClickedOnBuild;
    }

    private void InitializePrefabDictionaries()
    {
        // Initialize building prefab dictionary
        buildingPrefabDict = new Dictionary<BuildingType, Transform>();
        foreach (var entry in buildingPrefabs)
        {
            if (entry.prefab != null)
            {
                buildingPrefabDict[entry.type] = entry.prefab;
            }
        }
    }

    private void GameManager_OnClickedOnBuild(object sender, GameManager.OnClickedOnBuildEventArgs e)
    {
        // For now, defaulting to City building type
        // You'll want to modify this to get the building type from the event args
        SpawnBuilding(BuildingType.City, e.Tile.transform.position);
    }

    public void SpawnBuilding(BuildingType buildingType, Vector3 position)
    {
        if (buildingPrefabDict.TryGetValue(buildingType, out Transform prefab))
        {
            Vector3 adjustedPosition = new Vector3(position.x, position.y, position.z - 0.1f);
            Transform spawnedBuilding = Instantiate(prefab, adjustedPosition, Quaternion.identity);

            // Only spawn network object if it has NetworkObject component
            /* NetworkObject networkObj = spawnedBuilding.GetComponent<NetworkObject>();
            if (networkObj != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                networkObj.Spawn(true);
            } */
            NetworkObject networkObj = spawnedBuilding.GetComponent<NetworkObject>();
            networkObj.Spawn(true);
        }
        else
        {
            Debug.LogWarning($"No prefab found for building type: {buildingType}");
        }
    }

    public Transform GetBuildingPrefab(BuildingType buildingType)
    {
        buildingPrefabDict.TryGetValue(buildingType, out Transform prefab);
        return prefab;
    }

    // Helper method to initialize lists with all enum values (useful for setup)
    [ContextMenu("Initialize All Building Types")]
    private void InitializeAllBuildingTypes()
    {
        buildingPrefabs.Clear();
        foreach (BuildingType buildingType in Enum.GetValues(typeof(BuildingType)))
        {
            buildingPrefabs.Add(new BuildingPrefabEntry(buildingType));
        }
    }
}