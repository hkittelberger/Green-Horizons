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
        if (IsServer)
        {
            InitAllPlayers();
        }
    }

    public void InitAllPlayers()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsList;

        for (int i = 0; i < clients.Count; i++)
        {
            var client = clients[i];
            ulong clientId = client.ClientId;

            Color assignedColor = defaultPlayerColors.Count > i ? defaultPlayerColors[i] : UnityEngine.Random.ColorHSV();

            PlayerData data = new PlayerData(assignedColor);
            AllPlayerData[clientId] = data;

            Debug.Log($"[PlayerManager] Assigned Player {clientId} color {assignedColor}");

            // Sync data to client
            SyncPlayerDataClientRpc(clientId, assignedColor.r, assignedColor.g, assignedColor.b,
                data.water, data.alloy, data.oil, data.food, data.brick);
        }
    }

    [ClientRpc]
    private void SyncPlayerDataClientRpc(ulong clientId, float r, float g, float b, int water, int alloy, int oil, int food, int brick)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        Color color = new Color(r, g, b);
        var data = new PlayerData(color)
        {
            water = water,
            alloy = alloy,
            oil = oil,
            food = food,
            brick = brick
        };

        AllPlayerData[clientId] = data;
        Debug.Log($"[PlayerManager] Synced PlayerData on client {clientId} with color {color}");
    }
    
    public PlayerData GetPlayerData(ulong clientId)
    {
        return AllPlayerData.TryGetValue(clientId, out var data) ? data : null;
    }
}
