using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyListUI : MonoBehaviour
{
    [SerializeField] private Transform lobbyInfoTemplate;

    private void Awake()
    {
        GameLobby.Instance.OnUpdateLobbyList += Game_OnUpdateLobbyList;

        lobbyInfoTemplate.gameObject.SetActive(false);
    }

    private void Game_OnUpdateLobbyList(object sender, GameLobby.OnUpdateLobbyListEventArgs e)
    {
        foreach (Transform child in this.transform)
        {
            if (child == lobbyInfoTemplate)
            {
                continue;
            }
            else
            {
                Destroy(child.gameObject);
            }
        }

        foreach (Lobby lobby in e.lobbies)
        {
            Transform lobbyInfoSingleTransform = Instantiate(lobbyInfoTemplate, this.transform);

            LobbyInfoSingle lobbyInfoSingle = lobbyInfoSingleTransform.GetComponent<LobbyInfoSingle>();

            lobbyInfoSingle.SetLobby(lobby);

            lobbyInfoSingleTransform.gameObject.SetActive(true);
        }
    }
}
