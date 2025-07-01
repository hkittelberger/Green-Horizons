using UnityEngine;
using Unity.Netcode;
using System;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private Transform hexRenderPrefab;
    private PlayerManager playerManager;

    // Singleton-safe initialization
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    // Networking-safe initialization
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CreateBoard();
        }
    }

    // Ensures only one board is spawned by server
    private void CreateBoard()
    {
        if (hexRenderPrefab == null)
        {
            Debug.LogError("HexRenderPrefab is not assigned in GameManager.");
            return;
        }

        Vector3 origin = Vector3.zero;
        Transform hexRender = Instantiate(hexRenderPrefab, origin, Quaternion.identity);
        NetworkObject networkObject = hexRender.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.Spawn(true);
        }
        else
        {
            Debug.LogError("Spawned hexRenderPrefab does not have a NetworkObject.");
        }
    }

    // Called by UI or input code when player clicks to build
    public event EventHandler<OnClickedOnBuildEventArgs> OnClickedOnBuild;

    public class OnClickedOnBuildEventArgs : EventArgs
    {
        public BuildingType BuildTypeEnum;
        public HexTile Tile;
        public ulong BuilderClientId;
    }

    public void ClickedOnBuild(HexTile tile, BuildingType buildType)
    {
        if (tile == null)
        {
            Debug.LogError("Clicked tile is null.");
            return;
        }

        // If we're the server, handle the build directly
        if (IsServer)
        {
            HandleBuild(tile, buildType, NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            // If client, forward request to server
            var netObj = tile.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                RequestBuildServerRpc(netObj, buildType);
            }
            else
            {
                Debug.LogError("HexTile does not have a NetworkObject component.");
            }
        }
    }

    private void HandleBuild(HexTile tile, BuildingType buildType, ulong builderClientId)
    {
        Debug.Log($"[Server] Build requested: {buildType} on tile {tile.name} by client {builderClientId}");

        OnClickedOnBuild?.Invoke(this, new OnClickedOnBuildEventArgs
        {
            BuildTypeEnum = buildType,
            Tile = tile,
            BuilderClientId = builderClientId,
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestBuildServerRpc(NetworkObjectReference tileRef, BuildingType buildType, ServerRpcParams rpcParams = default)
    {
        if (!tileRef.TryGet(out NetworkObject tileObj))
        {
            Debug.LogError("Invalid tile reference sent to server.");
            return;
        }

        HexTile tile = tileObj.GetComponent<HexTile>();
        if (tile == null)
        {
            Debug.LogError("NetworkObject does not contain a HexTile component.");
            return;
        }

        ulong senderClientId = rpcParams.Receive.SenderClientId;

        HandleBuild(tile, buildType, senderClientId);
    }
}
