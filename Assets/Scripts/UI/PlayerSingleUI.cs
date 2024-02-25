using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerSingleUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] TextMeshProUGUI playerLivesText;

    [SerializeField] UnityEngine.UI.Image progressBar;

    [SerializeField] private int playerDataIndex;

    private bool setUp = false;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += Game_OnStateChanged;

        Hide();
    }

    private void Game_OnPlayerSelectingCardsFinished(object sender, System.EventArgs e)
    {
        progressBar.enabled = false;
    }

    private void Game_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataWithIndex(playerDataIndex);

        if (playerData.hasTurn)
        {
            SetPlayerNameTextColor(Color.green);
        }
        else if (GameMultiplayer.Instance.IsPlayerTroll(playerData))
        {
            SetPlayerNameTextColor(Color.red);
        }
        else
        {
            SetPlayerNameTextColor(Color.white);
        }

        SetPlayerLivesText(playerData.livesCount);
    }

    private void Game_OnPlayerTurnTimerChanged(object sender, GameManager.OnTimerChangedEventArgs e)
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataWithIndex(playerDataIndex);

        if (playerData.hasTurn)
        {
            progressBar.enabled = true;
            progressBar.fillAmount = e.timer / 15f;
        }
        else
        {
            progressBar.enabled = false;
        }
    }

    private void Game_OnTrollTurnTimerChanged(object sender, GameManager.OnTimerChangedEventArgs e)
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataWithIndex(playerDataIndex);

        if (GameMultiplayer.Instance.IsPlayerTroll(playerData))
        {
            progressBar.enabled = true;
            progressBar.fillAmount = e.timer / 5f;
        }
        else
        {
            progressBar.enabled = false;
        }
    }


    private void Game_OnNextTurn(object sender, System.EventArgs e)
    {
        progressBar.enabled = false;
    }
    
    private void Game_OnStateChanged(object sender, System.EventArgs e)
    {
        Debug.Log("Game_OnStateChanged event method called in PlayerSingleUI script");

        Debug.Log("res: " + GameMultiplayer.Instance.DoesPlayerDataListContainIndex(playerDataIndex));

        Debug.Log("playerDataIndex: " + playerDataIndex);

        if (GameMultiplayer.Instance.DoesPlayerDataListContainIndex(playerDataIndex))
        {
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataWithIndex(playerDataIndex);

            if (setUp)
            {
                if (playerData.hasTurn)
                {
                    SetPlayerNameTextColor(Color.green);
                }
                else if (GameMultiplayer.Instance.IsPlayerTroll(playerData))
                {
                    SetPlayerNameTextColor(Color.red);
                }
                else
                {
                    SetPlayerNameTextColor(Color.white);
                }
            }
            else
            {
                SetPlayerNameText(playerData.name.ToString());

                GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += Game_OnPlayerDataNetworkListChanged;

                GameMultiplayer.Instance.OnPlayerSelectingCardsFinished += Game_OnPlayerSelectingCardsFinished;

                GameManager.Instance.OnNextTurn += Game_OnNextTurn;

                GameManager.Instance.OnPlayerTurnTimerChanged += Game_OnPlayerTurnTimerChanged;

                GameManager.Instance.OnTrollTurnTimerChanged += Game_OnTrollTurnTimerChanged;

                Show();

                setUp = true;
            }
            SetPlayerLivesText(playerData.livesCount);
        }
        else
        {
            Hide();
        }
    }

    public void SetPlayerNameText(string playerNameText)
    {
        this.playerNameText.text = playerNameText;
    }

    public void SetPlayerNameTextColor(Color color)
    {
        this.playerNameText.color = color;
    }

    public void SetPlayerLivesText(int lives)
    {
        playerLivesText.text = "LIVES: " + lives.ToString();
    }

    private void Show()
    {
        this.gameObject.SetActive(true);
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= Game_OnPlayerDataNetworkListChanged;

        GameMultiplayer.Instance.OnPlayerSelectingCardsFinished -= Game_OnPlayerSelectingCardsFinished;
    }
}
