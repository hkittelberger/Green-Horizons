using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyAssets : MonoBehaviour {



    public static LobbyAssets Instance { get; private set; }


    [SerializeField] private Sprite redSprite;
    [SerializeField] private Sprite blueSprite;
    [SerializeField] private Sprite greenSprite;
    [SerializeField] private Sprite yellowSprite;
    [SerializeField] private Sprite unassignedSprite;


    private void Awake()
    {
        Instance = this;
    }

    public Sprite GetSprite(LobbyManager.PlayerCharacter playerCharacter) {
        switch (playerCharacter) {
            default:
            case LobbyManager.PlayerCharacter.Red:   return redSprite;
            case LobbyManager.PlayerCharacter.Blue:    return blueSprite;
            case LobbyManager.PlayerCharacter.Green:   return greenSprite;
            case LobbyManager.PlayerCharacter.Yellow:  return yellowSprite;
            case LobbyManager.PlayerCharacter.Unassigned: return unassignedSprite;
        }
    }

}