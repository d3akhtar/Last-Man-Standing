using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PostJoinLobbyReady : NetworkBehaviour
{
    public static PostJoinLobbyReady Instance { get; private set; }

    private Dictionary<ulong, bool> playerReadyDictionary;

    [SerializeField] private Button startGameButton;

    public event EventHandler<OnAnyPlayerReadyStateChangedEventArgs> OnAnyPlayerReadyStateChanged;

    public class OnAnyPlayerReadyStateChangedEventArgs : EventArgs
    {
        public ulong clientId;
        public bool ready;
    }

    private void Awake()
    {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        startGameButton.onClick.AddListener(() =>
        {
            Loader.LoadNetwork(Loader.Scene.GameScene);
        });

        startGameButton.gameObject.SetActive(false);
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;

        if (!playerReadyDictionary.ContainsKey(senderClientId))
        {
            playerReadyDictionary[senderClientId] = true;
        }
        else
        {
            playerReadyDictionary[senderClientId] = !playerReadyDictionary[senderClientId];
        }

        OnAnyPlayerReadyStateChangedEventCallClientRpc(senderClientId, playerReadyDictionary[senderClientId]);

        if (AllPlayersReady())
        {
            // enable start game button for host
            startGameButton.gameObject.SetActive(true);
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void OnAnyPlayerReadyStateChangedEventCallClientRpc(ulong clientId, bool ready)
    {
        OnAnyPlayerReadyStateChanged?.Invoke(this, new OnAnyPlayerReadyStateChangedEventArgs
        {
            clientId = clientId,
            ready = ready,
        });
    } 

    // Only serverRpc calls this
    private bool AllPlayersReady()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                return false;
            }
        }

        return true;
    }
}
