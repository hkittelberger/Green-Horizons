using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class SceneButtonLoader : MonoBehaviour
{

    public string sceneName;

    // Call this from a Button's OnClick to load a scene by name
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("NetworkManager is not available or not running as server.");
            }
        }
        else
        {
            Debug.LogWarning("Scene name is empty or null.");
        }
    }
}