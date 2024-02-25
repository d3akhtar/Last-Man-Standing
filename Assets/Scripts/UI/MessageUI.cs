using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MessageUI : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button closeButton;

    [SerializeField] private TextMeshProUGUI messageText;

    private void Start()
    {
        GameLobby.Instance.OnTryingToCreateLobby += Game_OnTryingToCreateLobby;
        GameLobby.Instance.OnTryingToJoinLobby += Game_OnTryingToJoinLobby;
        GameLobby.Instance.OnFailedToJoinLobby += Game_OnFailedToJoinLobby;

        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });

        Hide();
    }

    private void Game_OnFailedToJoinLobby(object sender, System.EventArgs e)
    {
        DisplayMessage("Failed to join lobby!");
    }

    private void Game_OnTryingToJoinLobby(object sender, System.EventArgs e)
    {
        DisplayMessage("Trying to join lobby...");
    }

    private void Game_OnTryingToCreateLobby(object sender, System.EventArgs e)
    {
        DisplayMessage("Trying to create lobby...");
    }

    private void DisplayMessage(string message)
    {
        messageText.text = message;
        Show();
    }
    private void Show()
    {
        this.gameObject.SetActive(true);
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
