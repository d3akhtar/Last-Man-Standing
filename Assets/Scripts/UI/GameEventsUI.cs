using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameEventsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI detailsText;

    private float showTimer = 0f;
    private float showTimerMax = 5f;

    private void Awake()
    {
        GameMultiplayer.Instance.OnSomeoneTurnStolen += Game_OnSomeoneTurnStolen;
        GameMultiplayer.Instance.OnSomeoneTrolled += Game_OnSomeoneTrolled;
        GameManager.Instance.OnGoldenRoundActivated += Game_OnGoldenRoundActivated;

        Hide();
    }

    private void Game_OnGoldenRoundActivated(object sender, System.EventArgs e)
    {
        showTimer = showTimerMax;

        ShowText("GOLDEN ROUND ACTIVATED! STAY SHARP.");
    }

    private void Game_OnSomeoneTrolled(object sender, GameMultiplayer.OnSomeoneTrolledEventArgs e)
    {
        showTimer = showTimerMax;

        if (e.troll_id == NetworkManager.Singleton.LocalClientId)
        {
            PlayerData trolledPlayerData = GameMultiplayer.Instance.GetPlayerDataWithClientId(e.trolled_id);

            ShowText("YOU TROLLED " + trolledPlayerData.name.ToString() + "!");
        }
        else if (e.trolled_id == NetworkManager.Singleton.LocalClientId)
        {
            PlayerData trollPlayerData = GameMultiplayer.Instance.GetPlayerDataWithClientId(e.troll_id);

            ShowText(trollPlayerData.name + " TROLLED YOU!");
        }
        else
        {
            PlayerData trollPlayerData = GameMultiplayer.Instance.GetPlayerDataWithClientId(e.troll_id);

            PlayerData trolledPlayerData = GameMultiplayer.Instance.GetPlayerDataWithClientId(e.trolled_id);

            ShowText(trollPlayerData.name.ToString() + " STOLE " + trolledPlayerData.name.ToString() + "'s TURN!");
        }
    }

    private void Update()
    {
        if (showTimer > 0)
        {
            showTimer -= Time.deltaTime;

            if (showTimer <= 0)
            {
                Hide();
            }
        }
    }

    private void Game_OnSomeoneTurnStolen(object sender, GameMultiplayer.OnSomeoneTurnStolenEventArgs e)
    {
        showTimer = showTimerMax;

        if (e.thief_id == NetworkManager.Singleton.LocalClientId)
        {
            PlayerData stolenFromPlayerData = GameMultiplayer.Instance.GetPlayerDataWithClientId(e.stolenFrom_id);

            ShowText("YOU STOLE " + stolenFromPlayerData.name.ToString() + "'s TURN!");
        }
        else if (e.stolenFrom_id == NetworkManager.Singleton.LocalClientId)
        {
            PlayerData thiefPlayerData = GameMultiplayer.Instance.GetPlayerDataWithClientId(e.thief_id);

            ShowText(thiefPlayerData.name + " STOLE YOUR TURN!");
        }
        else
        {
            PlayerData thiefPlayerData = GameMultiplayer.Instance.GetPlayerDataWithClientId(e.thief_id);

            PlayerData stolenFromPlayerData = GameMultiplayer.Instance.GetPlayerDataWithClientId(e.stolenFrom_id);

            ShowText(thiefPlayerData.name.ToString() + " STOLE " + stolenFromPlayerData.name.ToString() + "'s TURN!");
        }
    }

    private void ShowText(string text)
    {
        this.gameObject.SetActive(true);

        detailsText.text = text;
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnSomeoneTurnStolen -= Game_OnSomeoneTurnStolen;
        GameMultiplayer.Instance.OnSomeoneTrolled -= Game_OnSomeoneTrolled;
    }
}
