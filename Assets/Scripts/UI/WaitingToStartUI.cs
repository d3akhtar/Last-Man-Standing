using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingToStartUI : MonoBehaviour
{

    private void Start()
    {
        GameManager.Instance.OnLocalPlayerReady += Game_OnLocalPlayerReady;
        GameManager.Instance.OnStateChanged += Game_OnStateChanged;

        Hide();
    }

    private void Game_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsPlayerTurn())
        {
            Hide();
        }
    }

    private void Game_OnLocalPlayerReady(object sender, System.EventArgs e)
    {
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
