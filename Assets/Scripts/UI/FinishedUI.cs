using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FinishedUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Transform playAgainReadyVisualContainerTransform;
    [SerializeField] private Button newGameButton;

    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake()
    {
        playerReadyDictionary = new Dictionary<ulong, bool>();

        if (!NetworkManager.Singleton.IsServer)
        {
            foreach (Transform child in playAgainReadyVisualContainerTransform)
            {
                child.gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < playAgainReadyVisualContainerTransform.childCount; i++)
            {
                if (GameMultiplayer.Instance.DoesPlayerDataListContainIndex(i))
                {
                    playAgainReadyVisualContainerTransform.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    playAgainReadyVisualContainerTransform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
    }

    private void Start()
    {
        GameMultiplayer.Instance.OnLastManStanding += Game_OnLastManStanding;

        playAgainButton.onClick.AddListener(() =>
        {
            SetPlayerReadyServerRpc();
        });
        mainMenuButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        newGameButton.onClick.AddListener(() =>
        {
            GameManager.Instance.ResetGameState();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        });

        newGameButton.gameObject.SetActive(false);
        Hide();
    }

    private void Game_OnLastManStanding(object sender, GameMultiplayer.OnLastManStandingEventArgs e)
    {
        Show();
        SetWinnerText(e.winnerName);

        if (NetworkManager.Singleton.IsServer)
        {
            GameMultiplayer.Instance.ResetPlayerDatas();
        }
    }

    private void SetWinnerText(string winner)
    {
        winnerText.text = winner + " IS THE LAST MAN STANDING!";
    }

    private void Show()
    {
        this.gameObject.SetActive(true);
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        int index = GameMultiplayer.Instance.GetPlayerDataIndexWithClientId(senderClientId);

        if (!playerReadyDictionary.ContainsKey(senderClientId))
        {
            playerReadyDictionary[senderClientId] = true;
        }
        else
        {
            playerReadyDictionary[senderClientId] = !playerReadyDictionary[senderClientId];
        }

        if (playerReadyDictionary[senderClientId])
        {
            playAgainReadyVisualContainerTransform.GetChild(index).GetComponent<Image>().color = new Color(0, 255, 0);
        }
        else
        {
            playAgainReadyVisualContainerTransform.GetChild(index).GetComponent<Image>().color = Color.white;
        }

        bool everyoneReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                everyoneReady = false;
                break;
            }
        }

        if (everyoneReady)
        {
            newGameButton.gameObject.SetActive(true);
        }
        else
        {
            newGameButton.gameObject.SetActive(false);
        }

    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnLastManStanding -= Game_OnLastManStanding;
    }
}
