using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BuildingManager : NetworkBehaviour
{
    public static BuildingManager Instance { get; private set; }

    [System.Serializable]
    public class BuildingTypeEntry
    {
        public BuildingType type;
        public BuildingData definition;
    }

    [Header("All Building Prefabs")]
    [SerializeField] private List<BuildingTypeEntry> buildingDefinitions = new();
    private readonly Dictionary<BuildingType, BuildingData> definitionByType = new();

    private readonly List<Building> allSpawned = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeRegistry();
    }

    private void InitializeRegistry()
    {
        foreach (var entry in buildingDefinitions)
        {
            if (!definitionByType.ContainsKey(entry.type) && entry.definition != null)
            {
                definitionByType[entry.type] = entry.definition;
            }
        }
    }

    public BuildingData GetDefinition(BuildingType type)
    {
        definitionByType.TryGetValue(type, out var def);
        return def;
    }

    public GameObject GetPrefab(BuildingType type)
    {
        return GetDefinition(type)?.prefab;
    }

    public void RegisterBuilding(Building building)
    {
        if (!IsServer || building == null) return;
        allSpawned.Add(building);
        // Debug.Log($"[BuildingManager] Registered building: {building.GetDisplayName()}");
    }

    public List<Building> GetSpawnedBuildings() => allSpawned;
}
