using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BuildingVisualizer : NetworkBehaviour
{
    public static BuildingVisualizer Instance { get; private set; }

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
        if (IsServer && GameManager.Instance != null)
        {
            GameManager.Instance.OnClickedOnBuild += OnClickedOnBuild;
        }
    }

    private void OnClickedOnBuild(object sender, GameManager.OnClickedOnBuildEventArgs e)
    {
        SpawnBuildingRpc(e.BuildTypeEnum, e.Tile.transform.position + new Vector3(0, 0, -0.1f), e.BuilderClientId);
    }

    [Rpc(SendTo.Server)]
    public void SpawnBuildingRpc(BuildingType buildingType, Vector3 position, ulong clientId)
    {
        var prefab = BuildingManager.Instance.GetPrefab(buildingType);
        if (prefab == null)
        {
            Debug.LogError($"[BuildingVisualizer] No prefab found for type: {buildingType}");
            return;
        }

        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        var netObj = instance.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("Spawned prefab is missing NetworkObject.");
            return;
        }

        netObj.Spawn(true);

        if (instance.TryGetComponent(out Building building))
        {
            var color = PlayerManager.Instance?.GetPlayerData(clientId)?.playerColor ?? Color.gray;
            Debug.Log($"[BuildingVisualizer] Initializing building for client {clientId} with color {color}");

            building.Initialize(clientId, color);
            building.SetColorClientRpc(color.r, color.g, color.b, color.a);
            BuildingManager.Instance.RegisterBuilding(building);
        }
        else
        {
            Debug.LogWarning("Spawned prefab missing Building component.");
        }
    }
}
