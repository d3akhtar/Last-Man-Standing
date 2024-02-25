using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyStatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    [SerializeField] private Transform postJoinLobbyPlayerTemplateTransform;

    private Dictionary<ulong, GameObject> checkmarks = new Dictionary<ulong, GameObject>();
    private void Awake()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += Game_OnPlayerDataNetworkListChanged;

        postJoinLobbyPlayerTemplateTransform.gameObject.SetActive(false);
        lobbyCodeText.text = "Lobby Code: " + GameLobby.Instance.GetLobbyJoinCode();
    }

    private void Start()
    {
        PostJoinLobbyReady.Instance.OnAnyPlayerReadyStateChanged += PostJoinLobby_OnAnyPlayerReadyStateChanged;
        UpdatePlayerList();
    }

    private void PostJoinLobby_OnAnyPlayerReadyStateChanged(object sender, PostJoinLobbyReady.OnAnyPlayerReadyStateChangedEventArgs e)
    {
        checkmarks[e.clientId].SetActive(e.ready);
    }

    private void Game_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        Debug.Log("Updating player list...");

        checkmarks.Clear();

        foreach (Transform child in this.transform)
        {
            if (child == postJoinLobbyPlayerTemplateTransform)
            {
                continue;
            }
            else
            {
                Destroy(child.gameObject);
            }
        }

        NetworkList<PlayerData> playerDataList = GameMultiplayer.Instance.GetPlayerDataNetworkList();

        foreach (PlayerData playerData in playerDataList)
        {
            Transform postJoinLobbyPlayerUiTransform = Instantiate(postJoinLobbyPlayerTemplateTransform, this.transform);
            PostJoinLobbyPlayerSingle postJoinLobbyPlayerSingle = postJoinLobbyPlayerUiTransform.gameObject.
                GetComponent<PostJoinLobbyPlayerSingle>();

            postJoinLobbyPlayerSingle.SetPlayerNameText(playerData.name.ToString());

            postJoinLobbyPlayerSingle.ShowReadyButton(playerData.clientId == NetworkManager.Singleton.LocalClientId);

            postJoinLobbyPlayerUiTransform.gameObject.SetActive(true);

            checkmarks.Add(playerData.clientId, postJoinLobbyPlayerSingle.GetReadyCheckmarkGameObject());
        }
    }
    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= Game_OnPlayerDataNetworkListChanged;
    }
}
