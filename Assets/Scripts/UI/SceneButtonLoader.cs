using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneButtonLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad;

    [Header("Manager Prefabs")]
    [Tooltip("List of manager prefabs (must have a NetworkObject) that the server will spawn after loading the scene.")]
    public GameObject[] managerPrefabs;

    // Public method to hook up to button
    public void RequestSceneLoad()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            LoadSceneOnServer(sceneToLoad);
        }
        else
        {
            LoadSceneServerRpc(sceneToLoad);
        }
    }

    public void TestFunction()
    {
        Debug.Log("TestFunction called");
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadSceneServerRpc(string targetScene)
    {
        LoadSceneOnServer(targetScene);
    }

    private void LoadSceneOnServer(string targetScene)
    {
        if (string.IsNullOrWhiteSpace(targetScene))
        {
            Debug.LogWarning("Target scene name is empty.");
            return;
        }

        Debug.Log($"Server loading scene: {targetScene}");

        // Listen once for scene load complete
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= HandleSceneLoaded;
        NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleSceneLoaded;

        NetworkManager.Singleton.SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
    }

    private void HandleSceneLoaded(ulong clientId, string loadedScene, LoadSceneMode mode)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (!SceneManager.GetActiveScene().name.Equals(loadedScene)) return;

        Debug.Log($"Server finished loading scene: {loadedScene}");

        foreach (var prefab in managerPrefabs)
        {
            if (prefab == null) continue;

            // Optional: skip if already exists in scene by name
            if (GameObject.Find(prefab.name) != null)
            {
                Debug.LogWarning($"{prefab.name} already exists; skipping spawn.");
                continue;
            }

            GameObject instance = Instantiate(prefab);
            var netObj = instance.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(true);
                // Debug.Log($"{prefab.name} spawned by server.");
            }
            else
            {
                Debug.LogError($"{prefab.name} is missing a NetworkObject component.");
            }
        }

        // Unsubscribe to prevent stacking
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= HandleSceneLoaded;
    }
}
