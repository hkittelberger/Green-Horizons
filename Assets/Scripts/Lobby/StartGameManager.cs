using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class StartGameManager : MonoBehaviour
{

    [Header("Manager Prefabs (must have NetworkObject)")]
    [SerializeField] private GameObject[] managerPrefabs;

    [Header("Name of the scene to load via NetworkManager.SceneManager")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Local Camera (non-networked)")]
    [SerializeField] private GameObject playerCameraPrefab;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LobbyManager.Instance.OnLobbyStartGame += LobbyManager_OnLobbyStartGame;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // only run on clients, and only for our game scene
        if (!NetworkManager.Singleton.IsClient || scene.name != gameSceneName)
            return;

        SpawnLocalCamera();
    }

    private void LobbyManager_OnLobbyStartGame(object sender, LobbyManager.LobbyEventArgs e)
    {
        // Debug.Log("LobbyManager_OnLobbyStartGame called");
        if (LobbyManager.IsHost)
        {
            CreateRelayAndStart();
        }
        else
        {
            JoinRelay(LobbyManager.RelayJoinCode);
        }
    }

    // only once per load
    private void OnLoadComplete(ulong clientId, string scene, LoadSceneMode mode)
    {
        if (!NetworkManager.Singleton.IsServer || scene != gameSceneName) return;
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;

        // spawn your manager prefabs on the server
        foreach (var prefab in managerPrefabs)
        {
            if (prefab == null || GameObject.Find(prefab.name) != null) continue;
            var inst = Instantiate(prefab);
            var netObj = inst.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Spawn(true);
            else
                Debug.LogError($"'{prefab.name}' missing NetworkObject");
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


    private async void CreateRelayAndStart()
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(3);
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        var t = NetworkManager.Singleton.GetComponent<UnityTransport>();
        t.SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData
        );

        // Debug.Log($"Relay Join Code: {joinCode}");
        // Debug.Log("Starting Host with Relay...");
        StartHost();
        // Debug.Log("Host started successfully.");
        LobbyManager.Instance.SetRelayJoinCode(joinCode);

        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        NetworkManager.Singleton.SceneManager.LoadScene(
            gameSceneName,
            LoadSceneMode.Single
        );
    }

    private async void JoinRelay(string joinCode)
    {
        var joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

        var t = NetworkManager.Singleton.GetComponent<UnityTransport>();
        t.SetRelayServerData(
            joinAlloc.RelayServer.IpV4,
            (ushort)joinAlloc.RelayServer.Port,
            joinAlloc.AllocationIdBytes,
            joinAlloc.Key,
            joinAlloc.ConnectionData,
            joinAlloc.HostConnectionData
        );

        // Debug.Log("Joining Relay with Join Code: " + joinCode);
        // Debug.Log("Starting Client with Relay...");
        StartClient();
        // Debug.Log("Client started successfully.");
    }

    private void SpawnLocalCamera()
    {
        if (playerCameraPrefab == null)
        {
            Debug.LogError("PlayerCameraPrefab not assigned on StartGameManager.");
            return;
        }

        // figure out this client's index among connected clients
        var clients = NetworkManager.Singleton.ConnectedClientsList;
        var clientList = clients.ToList();
        int idx = clientList.FindIndex(c => c.ClientId == NetworkManager.Singleton.LocalClientId);

        // instantiate and set up
        var camObj = Instantiate(playerCameraPrefab);
        float startAngle = idx * 90f;              // 0°, 90°, 180°, 270° …
        camObj.transform.position = new Vector3(0f, 0f, -100f);
        camObj.transform.rotation = Quaternion.Euler(0, 0, startAngle);

        camObj.tag = "MainCamera";
    }
}