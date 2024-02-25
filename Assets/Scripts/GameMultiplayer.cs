using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMultiplayer : NetworkBehaviour
{
    public static GameMultiplayer Instance { get; private set; }

    public const int MAX_PLAYER_AMOUNT = 4;

    private NetworkList<PlayerData> playerDataNetworkList;

    private FixedString64Bytes DEFAULT_PLAYER_DATA_CHOSEN_CARD_TYPE = "none";

    public event EventHandler OnPlayerDataNetworkListChanged;
    public event EventHandler OnPlayerDataNameSet;
    public event EventHandler<OnPlayerSelectingCardsFinishedEventArgs> OnPlayerSelectingCardsFinished;
    public class OnPlayerSelectingCardsFinishedEventArgs : EventArgs
    {
        public bool isMatching;
    }

    public event EventHandler<OnSomeoneTurnStolenEventArgs> OnSomeoneTurnStolen;
    public class OnSomeoneTurnStolenEventArgs : EventArgs
    {
        public ulong stolenFrom_id;
        public ulong thief_id;
    }

    public event EventHandler<OnSomeoneTrolledEventArgs> OnSomeoneTrolled;
    public class OnSomeoneTrolledEventArgs : EventArgs
    {
        public ulong trolled_id;
        public ulong troll_id;
    }
    public event EventHandler OnTrollChoseCard;
    public event EventHandler OnCardEffectChosen;
    public event EventHandler<OnLastManStandingEventArgs> OnLastManStanding;
    public class OnLastManStandingEventArgs : EventArgs
    {
        public string winnerName;
    }

    private string playerName;
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";

    private NetworkVariable<int> indexOfPlayerDataWithTurn = new NetworkVariable<int>(-1);

    private ICard chosenCard;
    private ICard trollCard;
    public static ICard DEFAULT_CHOSEN_CARD = new _BaseCard();

    private void Awake()
    {
        chosenCard = DEFAULT_CHOSEN_CARD;
        trollCard = DEFAULT_CHOSEN_CARD;

        Instance = this;
        playerDataNetworkList = new NetworkList<PlayerData>();

        DontDestroyOnLoad(this.gameObject);

        SceneManager.activeSceneChanged += SceneManager_ActiveSceneChanged;

        playerName = PlayerPrefs.GetString
            (PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "playerName" + UnityEngine.Random.Range(0, 10000).ToString());
    }

    private void SceneManager_ActiveSceneChanged(Scene arg0, Scene arg1)
    {
        if (SceneManager.GetActiveScene().name == Loader.Scene.GameScene.ToString() && IsServer)
        {
            Debug.Log("GameScene loaded");
            Grid.Instance.OnGridFinishedSettingCards += Grid_OnGridFinishedSettingCards;
            GameManager.Instance.OnGoldenRoundActivated += Game_OnGoldenRoundActivated;
        }
    }

    private void Game_OnGoldenRoundActivated(object sender, EventArgs e)
    {
        int maxLiveCount = -1;
        foreach (PlayerData pd in playerDataNetworkList)
        {
            if (pd.livesCount > maxLiveCount)
            {
                maxLiveCount = pd.livesCount;
            }
        }
        SetGoldenRoundLivesServerRpc(maxLiveCount);
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetGoldenRoundLivesServerRpc(int maxLiveCount)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            
            if (playerData.livesCount == maxLiveCount)
            {
                playerData.livesCount = 2;
            }
            else
            {
                playerData.livesCount = 1;
            }

            playerDataNetworkList[i] = playerData;
        }
    }

    private void Grid_OnGridFinishedSettingCards(object sender, EventArgs e)
    {
        int i = 1;

        foreach (CardSpot cardspot in Grid.Instance.GetCardSpots())
        {
            cardspot.gameObject.name += " #" + i;
            cardspot.OnThisCardSelected += AnyCardSpot_OnThisCardSelected;
            cardspot.OnTrollSelectedThisCard += AnyCardSpot_OnTrollSelectedThisCard;

            i++;
        }
    }

    private void AnyCardSpot_OnTrollSelectedThisCard(object sender, CardSpot.OnThisCardSelectedEventArgs e)
    {
        SetPlayerDataFirstCardTypeUsingIndexServerRpc(e.card.GetCardType(), GetPlayerDataIndexOfTrolled());
    }

    private void AnyCardSpot_OnThisCardSelected(object sender, CardSpot.OnThisCardSelectedEventArgs e)
    {
        Debug.Log(sender.ToString());
        SetPlayerDataCardTypeServerRpc(e.card.GetCardType());
        SetChosenCardServerRpc(e.cardSpotIndex);
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetChosenCardServerRpc(int index)
    {
        SetChosenCardClientRpc(index);
    }

    [ClientRpc]
    private void SetChosenCardClientRpc(int index)
    {
        chosenCard = Grid.Instance.GetCardSpots()[index].GetCard();
    }

    [ClientRpc]
    private void SetChosenCardUsingCardGeneratorIndexClientRpc(int index)
    {
        Debug.Log("SetChosenCardUsingCardGeneratorIndexClientRpc() called with index arg being: " + index + " card type is " + GameManager.Instance.GenerateCardWithIndex(index).GetCardType());
        chosenCard = GameManager.Instance.GenerateCardWithIndex(index);
    }

    public bool IsPlayerTroll(PlayerData playerData)
    {
        int trollIndex = GetPlayerDataIndexOfTroll();

        if (trollIndex == -1) return false;

        return playerDataNetworkList[trollIndex].clientId == playerData.clientId;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetChosenCardServerRpc()
    { 
        ResetChosenCardClientRpc();
    }

    [ClientRpc]
    private void ResetChosenCardClientRpc()
    {
        chosenCard = DEFAULT_CHOSEN_CARD;
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetPlayerDataCardTypeServerRpc(string cardType)
    {
        Debug.Log("setting player card... cardType: " + cardType);

        PlayerData playerData = playerDataNetworkList[indexOfPlayerDataWithTurn.Value];

        if (playerData.firstCardType != DEFAULT_PLAYER_DATA_CHOSEN_CARD_TYPE)
        {
            Debug.Log("setting the second card");

            playerData.secondCardType = cardType;
            OnPlayerSelectingCardsFinishedEventCallClientRpc(playerData.firstCardType == playerData.secondCardType 
                                                                && playerData.firstCardType != DEFAULT_PLAYER_DATA_CHOSEN_CARD_TYPE);
        }
        else
        {
            Debug.Log("setting the first card");

            playerData.firstCardType = cardType;
        }

        playerDataNetworkList[indexOfPlayerDataWithTurn.Value] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerDataFirstCardTypeUsingIndexServerRpc(string cardType, int playerDataIndex)
    {
        Debug.Log("setting player card using index... cardType: " + cardType);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.firstCardType = cardType;

        playerData.trolled = false;

        playerData.playerDataIndexOfTroll = -1;

        playerDataNetworkList[playerDataIndex] = playerData;

        OnTrollChoseCard?.Invoke(this, EventArgs.Empty);
    }

    public void SetBothOfPlayerCards(int cardTypeGeneratorIndex)
    {
        SetBothOfPlayerCardsServerRpc(cardTypeGeneratorIndex);
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetBothOfPlayerCardsServerRpc(int cardTypeGeneratorIndex)
    {
        Debug.Log("cardTypeGeneratorIndex is " + cardTypeGeneratorIndex);

        SetChosenCardUsingCardGeneratorIndexClientRpc(cardTypeGeneratorIndex);

        OnCardEffectChosen?.Invoke(this, EventArgs.Empty);
    }


    [ClientRpc]
    private void OnPlayerSelectingCardsFinishedEventCallClientRpc(bool isMatching)
    {
        if (isMatching)
        {
            Debug.Log("chosenCard: " + chosenCard.GetCardType());
        }

        OnPlayerSelectingCardsFinished?.Invoke(this, new OnPlayerSelectingCardsFinishedEventArgs
        {
            isMatching = isMatching,
        });
    }
    public override void OnNetworkSpawn()
    {
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        Debug.Log("playerDataNetworkList was changed!");

        if (changeEvent.Type == NetworkListEvent<PlayerData>.EventType.Value)
        {
            Debug.Log("changed a value in this list");
        }

        if (changeEvent.Type == NetworkListEvent<PlayerData>.EventType.Add)
        {
           // idk...
        }

        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);

        if (IsOnePlayerLeft())
        {
            OnLastManStandingEventCallServerRpc(GetNameOfWinner());
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void OnLastManStandingEventCallServerRpc(string winnerName)
    {
        OnLastManStandingEventCallClientRpc(winnerName);
    }

    [ClientRpc]
    private void OnLastManStandingEventCallClientRpc(string winnerName)
    {
        OnLastManStanding?.Invoke(this, new OnLastManStandingEventArgs
        {
            winnerName = winnerName,
        });
    }

    public void StartHost()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;

        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnClientConnectedCallback(ulong id)
    {
        //Debug.Log("adding player...");

        playerDataNetworkList.Add(new PlayerData
        {
            clientId = id,
            hasTurn = false,
            turnIsStolen = false,
            trolled = false,
            playerDataIndexOfTroll = -1,
            livesCount = 4,
            firstCardType = "none",
            secondCardType = "none",
        });

        SetPlayerNameServerRpc(GetPlayerName());

        //Debug.Log("player count is now: " + playerDataNetworkList.Count);
    }

    public void ResetPlayerDatas()
    {
        ResetPlayerDatasServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    private void ResetPlayerDatasServerRpc()
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];

            playerData.hasTurn = false;
            playerData.turnIsStolen = false;
            playerData.trolled = false;
            playerData.playerDataIndexOfTroll = -1;
            playerData.livesCount = 4;
            playerData.firstCardType = "none";
            playerData.secondCardType = "none";

            playerDataNetworkList[i] = playerData;
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("Asking server to set player name to " + playerName + " . Call from: " + serverRpcParams.Receive.SenderClientId);

        int playerDataIndex = GetPlayerDataIndexWithClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.name = playerName;

        playerDataNetworkList[playerDataIndex] = playerData;

        Debug.Log("changed name... contents are:");
        foreach (PlayerData pd in playerDataNetworkList)
        {
            Debug.Log(pd.name);
        }

        CallSetNameEventClientRpc();
    }

    [ClientRpc]
    private void CallSetNameEventClientRpc()
    {
        OnPlayerDataNameSet?.Invoke(this, EventArgs.Empty);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;

        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong obj)
    {
        SetPlayerNameServerRpc(GetPlayerName());
    }

    public PlayerData GetPlayerDataWithClientId(ulong clientId)
    {
        //Debug.Log("getting playerData with clientId...");

        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }

        return default;
    }

    public PlayerData GetPlayerDataWithIndex(int index)
    {
        if (DoesPlayerDataListContainIndex(index))
        {
            return playerDataNetworkList[index];
        }

        //Debug.Log("index " + index + " not contained, returning default...");
        return default;
    }

    public int GetPlayerDataIndexWithClientId(ulong clientId)
    {
        //Debug.Log("getting playerDataIndex with clientId...");

        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetPlayerDataIndexOfTroll()
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].trolled)
            {
                return playerDataNetworkList[i].playerDataIndexOfTroll;
            }
        }

        return -1;
    }

    private int GetPlayerDataIndexOfTrolled()
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].trolled)
            {
                return i;
            }
        }

        return -1;
    }

    public bool DoesPlayerDataListContainIndex(int index)
    {
        return index < playerDataNetworkList.Count && index >= 0;
    }

    public bool IsLocalClientTurn()
    {
        //Debug.Log("is local client turn?");

        PlayerData playerData = GetPlayerDataWithClientId(NetworkManager.LocalClientId);

        return playerData.hasTurn;
    }

    public bool IsLocalClientTroll()
    {
        //Debug.Log("is local client turn?");

        int playerDataIndex = GetPlayerDataIndexWithClientId(NetworkManager.LocalClientId);

        int trollPlayerDataIndex = GetPlayerDataIndexOfTroll();

        return playerDataIndex == trollPlayerDataIndex;
    }

    public void SetPlayerTurnWithClientId(ulong clientId, bool hasTurn)
    {
        //Debug.Log("setting player turn... client id: " + clientId + " hasTurn: " + hasTurn);

        ResetPlayerDataCardsToNoneUsingIndexServerRpc(indexOfPlayerDataWithTurn.Value);

        indexOfPlayerDataWithTurn.Value = GetPlayerDataIndexWithClientId(clientId);

        PlayerData playerData = playerDataNetworkList[indexOfPlayerDataWithTurn.Value];

        if (hasTurn == true)
        {
            if (playerData.turnIsStolen)
            {
                PlayerData thiefPlayerData = playerDataNetworkList[playerData.playerDataIndexOfThief];

                thiefPlayerData.hasTurn = true;

                playerDataNetworkList[playerData.playerDataIndexOfThief] = thiefPlayerData;

                OnSomeoneTurnStolenEventCallServerRpc(playerData.clientId, thiefPlayerData.clientId);

                playerData.turnIsStolen = false;

                playerData.playerDataIndexOfThief = -1;

                playerDataNetworkList[indexOfPlayerDataWithTurn.Value] = playerData;

                indexOfPlayerDataWithTurn.Value = GetPlayerDataIndexWithClientId(thiefPlayerData.clientId);
            }
            else if (playerData.trolled)
            {
                Debug.Log("else if (playerData.trolled)");
                PlayerData trollPlayerData = playerDataNetworkList[playerData.playerDataIndexOfTroll];

                playerData.hasTurn = true;

                playerDataNetworkList[indexOfPlayerDataWithTurn.Value] = playerData;

                OnSomeoneTrolledEventCallServerRpc(playerData.clientId, trollPlayerData.clientId);

                //indexOfPlayerDataWithTurn.Value = GetPlayerDataIndexWithClientId(thiefPlayerData.clientId);
            }
            else
            {
                playerData.hasTurn = true;

                playerDataNetworkList[indexOfPlayerDataWithTurn.Value] = playerData;
            }
        }

        else
        {
            playerData.hasTurn = false;

            playerDataNetworkList[indexOfPlayerDataWithTurn.Value] = playerData;
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void OnSomeoneTurnStolenEventCallServerRpc(ulong stolenFrom_id, ulong thief_id)
    {
        Debug.Log("turn stolen event");
        OnSomeoneTurnStolenEventCallClientRpc(stolenFrom_id, thief_id);
    }

    [ClientRpc]
    private void OnSomeoneTurnStolenEventCallClientRpc(ulong stolenFrom_id, ulong thief_id)
    {
        OnSomeoneTurnStolen?.Invoke(this, new OnSomeoneTurnStolenEventArgs
        {
            stolenFrom_id = stolenFrom_id,
            thief_id = thief_id,
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnSomeoneTrolledEventCallServerRpc(ulong trolled_id, ulong troll_id)
    {
        Debug.Log("troll event");
        OnSomeoneTrolledEventCallClientRpc(trolled_id, troll_id);
    }

    [ClientRpc]
    private void OnSomeoneTrolledEventCallClientRpc(ulong trolled_id, ulong troll_id)
    {
        OnSomeoneTrolled?.Invoke(this, new OnSomeoneTrolledEventArgs
        {
            trolled_id = trolled_id,
            troll_id = troll_id,
        });
    }


    [ServerRpc (RequireOwnership = false)]
    private void ResetPlayerDataCardsToNoneUsingIndexServerRpc(int index)
    {
        ResetChosenCardServerRpc();

        if (index == -1)
            return;

        PlayerData playerData = playerDataNetworkList[index];

        playerData.firstCardType = DEFAULT_PLAYER_DATA_CHOSEN_CARD_TYPE;
        playerData.secondCardType = DEFAULT_PLAYER_DATA_CHOSEN_CARD_TYPE;

        playerDataNetworkList[index] = playerData;
    }

    public NetworkList<PlayerData> GetPlayerDataNetworkList()
    {
        Debug.Log("getting playerData list... contents are:");
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            Debug.Log(playerData.name);
        }

        return playerDataNetworkList;
    }

    public ICard GetChosenCard()
    {
        return chosenCard;
    }

    public void ReplacePlayerDataUsingIndex(string action, int index, int senderPlayerDataIndex = -1)
    {
        if (senderPlayerDataIndex == -1)
        {
            ReplacePlayerDataUsingIndexServerRpc(action, index);
        }
        else
        {
            ReplacePlayerDataUsingIndexServerRpc(action, index, senderPlayerDataIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReplacePlayerDataUsingIndexServerRpc(string action, int chosenPlayerIndex)
    {
        PlayerData chosenPlayerData = GetPlayerDataWithIndex(chosenPlayerIndex);

        switch(action)
        {
            case "ADD_LIFE":
                chosenPlayerData.livesCount++;
                break;
            case "SUBTRACT_LIFE":
                chosenPlayerData.livesCount--;
                break;

        }

        playerDataNetworkList[chosenPlayerIndex] = chosenPlayerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReplacePlayerDataUsingIndexServerRpc(string action, int chosenPlayerIndex, int senderPlayerDataIndex)
    {
        PlayerData chosenPlayerData = GetPlayerDataWithIndex(chosenPlayerIndex);
        PlayerData senderPlayerData = GetPlayerDataWithIndex(senderPlayerDataIndex);

        switch (action)
        {
            case "STEAL_TURN":
                chosenPlayerData.turnIsStolen = true;
                chosenPlayerData.playerDataIndexOfThief = senderPlayerDataIndex;
                break;
            case "STEAL_LIFE":
                chosenPlayerData.livesCount--;
                senderPlayerData.livesCount++;
                break;
            case "TROLL":
                chosenPlayerData.trolled = true;
                chosenPlayerData.playerDataIndexOfTroll = senderPlayerDataIndex;
                break;
        }

        playerDataNetworkList[chosenPlayerIndex] = chosenPlayerData;
        playerDataNetworkList[senderPlayerDataIndex] = senderPlayerData;
    }

    private string GetPlayerName()
    {
        return playerName;
    }

    public int GetRandomPlayerDataIndexExcludingGiven(int excludedIndex)
    {
        int res = excludedIndex;
        while (res == excludedIndex)
        {
            res = UnityEngine.Random.Range(0, playerDataNetworkList.Count);
        }

        return res;
    }

    public int GetPlayerCount()
    {
        return playerDataNetworkList.Count;
    }

    public string GetNameOfWinner()
    {
        foreach (PlayerData pd in playerDataNetworkList)
        {
            if (pd.livesCount > 0)
            {
                return pd.name.ToString();
            }
        }

        return "[NO WINNER YET]";
    }

    public int GetNumberOfPlayersAlive()
    {
        int res = 0;
        foreach (PlayerData pd in playerDataNetworkList)
        {
            if (pd.livesCount > 0)
            {
                res++;
            }
        }
        return res;
    }

    public bool IsPlayerDataWithThisClientIdAlive(ulong clientId)
    {
        foreach (PlayerData pd in playerDataNetworkList)
        {
            if (pd.clientId == clientId)
            {
                return pd.livesCount != 0;
            }
        }

        return false;
    }

    private bool IsOnePlayerLeft()
    {
        int numberOfPlayersAlive = 0;

        foreach (PlayerData pd in playerDataNetworkList)
        {
            if (pd.livesCount > 0)
            {
                numberOfPlayersAlive++;
                if (numberOfPlayersAlive > 1) return false;
            }
        }

        return true;
    }
}
