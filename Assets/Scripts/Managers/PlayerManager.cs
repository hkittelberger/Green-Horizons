using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using System.Collections;
using System.Linq;


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
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
         Debug.Log($"[PlayerManager] OnNetworkSpawn() IsServer={IsServer} IsClient={IsClient}");

        // if (IsServer)
        // {
        //     // existing server logic…
        //     foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        //         AssignAndSync(client.ClientId);
        //     NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        // }
    
           if (IsClient)
           {
               // client will auto‐report its lobby‐chosen color
               StartCoroutine(SubmitChosenColorWhenReady());
           }
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

    [ServerRpc(RequireOwnership = false)]
    public void SubmitChosenColorServerRpc(PlayerType chosen, ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;

        Color c = chosen switch {
            PlayerType.Red    => Color.red,
            PlayerType.Blue   => Color.blue,
            PlayerType.Green  => Color.green,
            PlayerType.Yellow => Color.yellow,
            _                 => Color.white
        };

        var data = new PlayerData(c);
        AllPlayerData[clientId] = data;

        // confirm back to that same client
        var clientRpcParams = new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = new[] { clientId }
            }
        };
        SyncPlayerDataClientRpc(c.r, c.g, c.b,
            data.water, data.alloy, data.oil, data.food, data.brick,
            clientRpcParams);
    }


    private IEnumerator SubmitChosenColorWhenReady()
    {
        Debug.Log("[PlayerManager] Waiting for LobbyManager + joinedLobby…");
        // 1) wait until LobbyManager exists & has a lobby
        while (LobbyManager.Instance == null
            || LobbyManager.Instance.GetJoinedLobby() == null)
        {
            yield return null;
        }

        Debug.Log("[PlayerManager] joinedLobby found, now waiting for your Character to be != Unassigned…");

        PlayerType chosenType;
        while (true)
        {
            // *** GRAB FRESH LOBBY EACH LOOP ***
            var lobby = LobbyManager.Instance.GetJoinedLobby();

            // find your player entry
            var me = lobby.Players
                        .First(p => p.Id ==
                            Unity.Services.Authentication.AuthenticationService.Instance.PlayerId);

            // parse their current DataObject
            var rawString = me.Data[LobbyManager.KEY_PLAYER_CHARACTER].Value;
            if (Enum.TryParse<LobbyManager.PlayerCharacter>(rawString, out var raw)
                && raw != LobbyManager.PlayerCharacter.Unassigned)
            {
                Debug.Log($"[PlayerManager] Detected your lobby color = {raw}");
                // map to our PlayerType
                if (Enum.TryParse<PlayerType>(raw.ToString(), out chosenType))
                    break;
                else
                    Debug.LogError($"[PlayerManager] Could not map raw '{raw}' to PlayerType");
            }

            // still unassigned → wait a frame
            yield return null;
        }

        Debug.Log($"[PlayerManager] Firing SubmitChosenColorServerRpc({chosenType})");
        SubmitChosenColorServerRpc(chosenType);
    }
    
    public PlayerData GetPlayerData(ulong clientId)
    {
        return AllPlayerData.TryGetValue(clientId, out var data) ? data : null;
    }
}
