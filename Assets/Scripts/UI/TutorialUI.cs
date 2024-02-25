using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{

    private void Awake()
    {
        Show();
    }
    private void Start()
    {
        GameManager.Instance.OnLocalPlayerReady += Game_OnLocalPlayerReady;
    }

    private void Game_OnLocalPlayerReady(object sender, System.EventArgs e)
    {
        Hide();
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
