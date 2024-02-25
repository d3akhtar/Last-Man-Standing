using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInfoSingle : MonoBehaviour
{
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private TextMeshProUGUI lobbyNameText;

    private Lobby lobby;

    private void Awake()
    {
        joinLobbyButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.JoinLobbyById(lobby.Id);
        });
    }

    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        SetLobbyNameText();
    }

    private void SetLobbyNameText()
    {
        lobbyNameText.text = lobby.Name;
    }
}
