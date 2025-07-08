using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;


public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public enum PlayerType
    {
        None,
        Red,
        Blue,
        Green,
        Yellow
    }

    // Public access to all players' data by client ID
    public Dictionary<ulong, PlayerData> AllPlayerData { get; private set; } = new();

    [Header("Default Colors for Players (by join order)")]
    [SerializeField]
    private List<Color> defaultPlayerColors = new()
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };

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

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Assign the host (client 0) and any already-connected clients:
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            AssignAndSync(client.ClientId);
        }

        // When new clients join later:
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        AssignAndSync(clientId);
    }

    private void AssignAndSync(ulong clientId)
    {
        // Pick a color by join order:
        int index = AllPlayerData.Count;
        Color assigned = index < defaultPlayerColors.Count
            ? defaultPlayerColors[index]
            : UnityEngine.Random.ColorHSV();

        var data = new PlayerData(assigned);
        AllPlayerData[clientId] = data;

        Debug.Log($"[PlayerManager] Server → assigned Client {clientId} color {assigned}");

        // Send a _targeted_ RPC to that client only:
        var rpcParams = new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = new[] { clientId }
            }
        };

        SyncPlayerDataClientRpc(
            assigned.r, assigned.g, assigned.b,
            data.water, data.alloy, data.oil, data.food, data.brick,
            rpcParams
        );
    }

    [ClientRpc]
    private void SyncPlayerDataClientRpc(
        float r, float g, float b,
        int water, int alloy, int oil, int food, int brick,
        ClientRpcParams rpcParams = default
    )
    {
        // This runs only on the target client:
        var color = new Color(r, g, b);
        var data  = new PlayerData(color)
        {
            water = water,
            alloy = alloy,
            oil   = oil,
            food  = food,
            brick = brick
        };

        AllPlayerData[NetworkManager.Singleton.LocalClientId] = data;
        Debug.Log($"[PlayerManager] Client {NetworkManager.Singleton.LocalClientId} ← synced data with color {color}");
    }
    
    public PlayerData GetPlayerData(ulong clientId)
    {
        return AllPlayerData.TryGetValue(clientId, out var data) ? data : null;
    }
}
