using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectSingleUI : MonoBehaviour
{
    private int playerDataIndex = -1;

    [SerializeField] private Button chooseButton;
    [SerializeField] private TextMeshProUGUI livesText;

    private void Awake()
    {
        chooseButton.onClick.AddListener(() =>
        {
            if (playerDataIndex == -1)
            {
                Debug.LogError("playerDataIndex not set in PlayerSelectSingleUI on " + this.name);
            }
            else
            {
                GameManager.Instance.SetChosenPlayerIndex(playerDataIndex);
            }
        });
    }

    public void SetPlayerDataIndex(int index)
    {
        playerDataIndex = index;

        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataWithIndex(playerDataIndex);

        chooseButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = playerData.name.ToString();
        livesText.text = "LIVES: " + playerData.livesCount.ToString();

        Show();
    }

    public int GetPlayerDataIndex()
    {
        return playerDataIndex;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

}
