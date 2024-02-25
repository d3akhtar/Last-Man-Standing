using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectUI : MonoBehaviour
{
    [SerializeField] private PlayerSelectSingleUI[] playerSelectSingles = new PlayerSelectSingleUI[3];
    [SerializeField] private Image timerBar;

    private void Awake()
    {
        foreach (PlayerSelectSingleUI playerSelectSingleUI in playerSelectSingles)
        {
            playerSelectSingleUI.Hide();
        }

        int localPlayerDataIndex = GameMultiplayer.Instance.GetPlayerDataIndexWithClientId(NetworkManager.Singleton.LocalClientId);
        int playerCount = GameMultiplayer.Instance.GetPlayerCount();

        for (int i = 0; i < playerCount; i++)
        {
            if (i == localPlayerDataIndex)
            {
                continue;
            }
            else
            {
                playerSelectSingles[i].SetPlayerDataIndex(i);
            }
        }
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerChoosingAnotherPlayer += Game_OnPlayerChoosingAnotherPlayer;
        GameManager.Instance.OnNextTurn += Game_OnNextTurn;
        GameManager.Instance.OnPlayerChooseOtherPlayerTimerChanged += Game_OnPlayerChooseOtherPlayerTimerChanged;

        Hide();
    }

    private void Game_OnPlayerChooseOtherPlayerTimerChanged(object sender, GameManager.OnTimerChangedEventArgs e)
    {
        timerBar.fillAmount = e.timer / 10f;
    }

    private void Game_OnNextTurn(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void Game_OnPlayerChoosingAnotherPlayer(object sender, System.EventArgs e)
    {
        if (!GameMultiplayer.Instance.IsLocalClientTurn())
        {
            Hide();
            timerBar.enabled = false;
        }
        else
        {
            Show();
            timerBar.enabled = true;
        }
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
