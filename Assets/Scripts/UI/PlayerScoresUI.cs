using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScoresUI : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.OnStateChanged += Game_OnStateChanged;
    }

    private void Game_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsPlayerTurn())
        {
            Show();
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
