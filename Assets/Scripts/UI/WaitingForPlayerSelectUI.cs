using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingForPlayerSelectUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image timerBar;

    private void Start()
    {
        GameManager.Instance.OnPlayerChooseOtherPlayerTimerChanged += Game_OnPlayerChooseOtherPlayerTimerChanged;
        GameManager.Instance.OnPlayerChoosingAnotherPlayer += Game_OnPlayerChoosingAnotherPlayer;
        GameManager.Instance.OnNextTurn += Game_OnNextTurn;

        Hide();
    }

    private void Game_OnNextTurn(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void Game_OnPlayerChoosingAnotherPlayer(object sender, System.EventArgs e)
    {
        if (GameMultiplayer.Instance.IsLocalClientTurn())
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

    private void Game_OnPlayerChooseOtherPlayerTimerChanged(object sender, GameManager.OnTimerChangedEventArgs e)
    {
        timerBar.fillAmount = e.timer / 10f; // 15f is the maximum amount of time for the player to choose another player
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
