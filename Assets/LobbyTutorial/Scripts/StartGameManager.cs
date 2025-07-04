using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;

public class StartGameManager : MonoBehaviour
{

    private void Start()
    {
        LobbyManager.Instance.OnLobbyStartGame += LobbyManager_OnLobbyStartGame;
    }

    private void LobbyManager_OnLobbyStartGame(object sender, LobbyManager.LobbyEventArgs e)
    {
        Debug.Log("LobbyManager_OnLobbyStartGame called");
        // Start Game!
        if (LobbyManager.IsHost)
        {
            CreateRelayAsync();
        }
        else
        {
            JoinRelayAsync(LobbyManager.RelayJoinCode);
        }
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }


    private async void CreateRelayAsync()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Allocated Relay JoinCode: " + joinCode);

            // Configure transport
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            StartHost();

            LobbyManager.Instance.SetRelayJoinCode(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinRelayAsync(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

}