using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Sprite[] cardSpriteArray = new Sprite[9];

    // sprite indexes
    public static int BACK_OF_CARD_SPRITE_INDEX = 0;
    public static int AXE_CARD_SPRITE_INDEX = 1;
    public static int FORCE_SHUFFLE_CARD_SPRTE_INDEX = 2;
    public static int LIFE_GAIN_CARD_SPRITE_INDEX = 3;
    public static int LIFE_STEAL_CARD_SPRITE_INDEX = 4;
    public static int MINE_CARD_SPRTE_INDEX = 5;
    public static int TROLL_CARD_SPRITE_INDEX = 6;
    public static int TURN_STEAL_CARD_SPRITE_INDEX = 7;
    public static int X_CARD_SPRITE_INDEX = 8;
    //

    private delegate ICard GenerateCard();

    private GenerateCard[] cardGenerator = new GenerateCard[8];

    private ICard[] graveyard = new ICard[8];

    private enum State
    {
        WAITING_TO_START,
        PLAYER_TURN,
        EVALUATING_CHOSEN_CARDS,
        PLAYER_SELECTING_OTHER_PLAYER, // state for when a card's effect involves player doing something to another player
        TROLL_CARD_STATE,
        X_CARD_CHOOSING_CARD_STATE,
        CARDS_FINISHED,
        FINISHED,
    };

    private Dictionary<ulong, bool> playerReadyDictionary;
    private bool isLocalPlayerReady = false;

    private bool sameCards = false;

    // events
    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalPlayerReady;
    public event EventHandler OnNextTurn;
    public event EventHandler<OnTimerChangedEventArgs> OnPlayerTurnTimerChanged;
    public event EventHandler<OnTimerChangedEventArgs> OnTrollTurnTimerChanged;
    public event EventHandler<OnTimerChangedEventArgs> OnPlayerChooseOtherPlayerTimerChanged;
    public event EventHandler<OnTimerChangedEventArgs> OnPlayerChooseCardTimerChanged;
    public class OnTimerChangedEventArgs : EventArgs
    {
        public float timer;
    }

    public event EventHandler OnCardsMatching;
    public event EventHandler OnCardsNotMatching;

    public event EventHandler OnPlayerChoosingAnotherPlayer;

    public event EventHandler OnGoldenRoundActivated;

    //

    private NetworkVariable<State> state = new NetworkVariable<State>();

    private NetworkVariable<float> playerTurnTimer = new NetworkVariable<float>(15f);
    private float playerTurnTimerMax = 15f; // this will be 15f later

    private NetworkVariable<float> evaluateChosenCardtimer = new NetworkVariable<float>(1f);
    private float evaluateChosenCardtimerMax = 1f;

    // time for player to choose another player for a card effet
    private NetworkVariable<float> playerChooseOtherPlayerTimer = new NetworkVariable<float>(10f);
    private float playerChooseOtherPlayerTimerMax = 10f;

    private NetworkVariable<float> trollTimer = new NetworkVariable<float>(5f);
    private float trollTimerMax = 5f;

    private NetworkVariable<float> chooseCardTimer = new NetworkVariable<float>(5f);
    private float chooseCardTimerMax = 5f;

    private NetworkVariable<int> connectedClientIdTurnIndex = new NetworkVariable<int>(0);

    private const int NOT_CHOSEN_INDEX = -1;
    private int chosenPlayerIndex = NOT_CHOSEN_INDEX;

    private int cardsUsed = 0;
    private int maxCards = 36;

    private void Awake()
    {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        playerTurnTimer.OnValueChanged += PlayerTurnTimer_OnValueChanged;
        playerChooseOtherPlayerTimer.OnValueChanged += PlayerChooseOtherPlayerTimer_OnValueChanged;
        trollTimer.OnValueChanged += TrollTimer_OnValueChanged;
        chooseCardTimer.OnValueChanged += ChooseCardTimer_OnValueChanged;
    }

    private void ChooseCardTimer_OnValueChanged(float previousValue, float newValue)
    {
        OnPlayerChooseCardTimerChanged?.Invoke(this, new OnTimerChangedEventArgs
        {
            timer = previousValue
        });
    }

    private void TrollTimer_OnValueChanged(float previousValue, float newValue)
    {
        OnTrollTurnTimerChanged?.Invoke(this, new OnTimerChangedEventArgs
        {
            timer = previousValue
        });
    }

    private void PlayerTurnTimer_OnValueChanged(float previousValue, float newValue)
    {
        OnPlayerTurnTimerChanged?.Invoke(this, new OnTimerChangedEventArgs
        {
            timer = previousValue
        });
    }

    private void PlayerChooseOtherPlayerTimer_OnValueChanged(float previousValue, float newValue)
    {
        OnPlayerChooseOtherPlayerTimerChanged?.Invoke(this, new OnTimerChangedEventArgs
        {
            timer = previousValue
        });
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    private void Update()
    {
        if (IsServer)
        {
            switch (state.Value)
            {
                case State.WAITING_TO_START:
                    break;

                case State.PLAYER_TURN:

                    playerTurnTimer.Value -= Time.deltaTime;

                    if (cardsUsed == maxCards)
                    {
                        state.Value = State.CARDS_FINISHED;
                    }

                    if (playerTurnTimer.Value <= 0f)
                    {
                        // go to next state to check player cards to see if they chose cards or if the cards match
                        state.Value = State.EVALUATING_CHOSEN_CARDS;
                    }
                    break;

                case State.EVALUATING_CHOSEN_CARDS:

                    evaluateChosenCardtimer.Value -= Time.deltaTime;

                    if (evaluateChosenCardtimer.Value < 0f)
                    {
                        evaluateChosenCardtimer.Value = evaluateChosenCardtimerMax;

                        if (sameCards)
                        {
                            cardsUsed += 2;

                            OnCardsMatching?.Invoke(this, EventArgs.Empty);

                            ICard card = GameMultiplayer.Instance.GetChosenCard();
                            Debug.Log("cardType: " + card.GetCardType());

                            if (!card.RequiresChoosingPlayer())
                            {
                                ulong mainPlayerClientId = NetworkManager.Singleton.ConnectedClientsIds[connectedClientIdTurnIndex.Value];
                                int mainPlayerIndex = GameMultiplayer.Instance.GetPlayerDataIndexWithClientId(mainPlayerClientId);
                                chosenPlayerIndex = NOT_CHOSEN_INDEX;

                                card.CardEffect(mainPlayerIndex, chosenPlayerIndex);

                                // after checking cards, if the card doesn't require player to select another player, give turn to next player
                                if (card.GetCardType() != "X")
                                {
                                    GoNextPlayerTurn();
                                }
                                else
                                {
                                    // Force the state if it hasn't already been updated by the event
                                    cardsUsed -= 2;
                                    state.Value = State.X_CARD_CHOOSING_CARD_STATE;
                                }
                            }
                            else
                            {
                                state.Value = State.PLAYER_SELECTING_OTHER_PLAYER;
                                OnPlayerChoosingAnotherPlayerEventCallClientRpc();
                            }
                        }
                        else
                        {
                            OnCardsNotMatching?.Invoke(this, EventArgs.Empty);

                            // after checking cards, if the card don't match, give turn to next player
                            GoNextPlayerTurn();
                        }
                    }

                    break;

                case State.PLAYER_SELECTING_OTHER_PLAYER:

                    playerChooseOtherPlayerTimer.Value -= Time.deltaTime;

                    if (playerChooseOtherPlayerTimer.Value <= 0)
                    {
                        playerChooseOtherPlayerTimer.Value = playerChooseOtherPlayerTimerMax;

                        ICard card = GameMultiplayer.Instance.GetChosenCard();

                        ulong mainPlayerClientId = NetworkManager.Singleton.ConnectedClientsIds[connectedClientIdTurnIndex.Value];
                        int mainPlayerIndex = GameMultiplayer.Instance.GetPlayerDataIndexWithClientId(mainPlayerClientId);

                        if (chosenPlayerIndex == -1)
                        {
                            chosenPlayerIndex = GameMultiplayer.Instance.GetRandomPlayerDataIndexExcludingGiven(mainPlayerIndex);
                        }

                        card.CardEffect(mainPlayerIndex, chosenPlayerIndex);

                        GoNextPlayerTurn();
                    }

                    break;

                case State.TROLL_CARD_STATE:

                    trollTimer.Value -= Time.deltaTime;
                    if (trollTimer.Value <= 0)
                    {
                        trollTimer.Value = trollTimerMax;

                        state.Value = State.PLAYER_TURN;
                    }
                    break;

                case State.X_CARD_CHOOSING_CARD_STATE:

                    chooseCardTimer.Value -= Time.deltaTime;
                    if (chooseCardTimer.Value <= 0)
                    {
                        chooseCardTimer.Value = chooseCardTimerMax;

                        state.Value = State.EVALUATING_CHOSEN_CARDS;
                    }
                    break;

                case State.CARDS_FINISHED:

                    if (GameMultiplayer.Instance.GetNumberOfPlayersAlive() == 1)
                    {
                        state.Value = State.FINISHED;
                    }
                    else
                    {
                        OnGoldenRoundActivatedEventCallServerRpc();
                        state.Value = State.PLAYER_TURN;
                        cardsUsed = 0;
                    }
                    break;

                case State.FINISHED:
                    break;
            }
        }
    }

    private void GoNextPlayerTurn()
    {
        GameMultiplayer.Instance.SetPlayerTurnWithClientId(NetworkManager.Singleton.ConnectedClientsIds[connectedClientIdTurnIndex.Value], false);

        while (true)
        {
            connectedClientIdTurnIndex.Value++;

            if (connectedClientIdTurnIndex.Value >= NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                connectedClientIdTurnIndex.Value = 0;
            }

            ulong clientId = NetworkManager.Singleton.ConnectedClientsIds[connectedClientIdTurnIndex.Value];

            if (GameMultiplayer.Instance.IsPlayerDataWithThisClientIdAlive(clientId)) { break; }
        }

        ulong turnClientId = NetworkManager.Singleton.ConnectedClientsIds[connectedClientIdTurnIndex.Value];

        GameMultiplayer.Instance.SetPlayerTurnWithClientId(turnClientId, true);

        OnNextTurnEventCallClientRpc();

        playerTurnTimer.Value = playerTurnTimerMax;

        chosenPlayerIndex = NOT_CHOSEN_INDEX;

        Debug.Log("changing state to player turn");
        if (state.Value != State.TROLL_CARD_STATE)
        {
            state.Value = State.PLAYER_TURN;
        }
    }

    private void Start()
    {
        SetCardGenerators();

        state.Value = State.WAITING_TO_START;

        GameInput.Instance.OnInteractPerformed += Player_OnInteractPerformed;

        // only server changes turns, so only server needs to listen to this event
        if (NetworkManager.LocalClientId == NetworkManager.ServerClientId)
        {
            GameMultiplayer.Instance.OnPlayerSelectingCardsFinished += Game_OnPlayerSelectingCardsFinished;
            GameMultiplayer.Instance.OnSomeoneTurnStolen += Game_OnSomeoneTurnStolen;
            GameMultiplayer.Instance.OnSomeoneTrolled += Game_OnSomeoneTrolled;
            GameMultiplayer.Instance.OnTrollChoseCard += Game_OnTrollChoseCard;
            GameMultiplayer.Instance.OnCardEffectChosen += Game_OnCardEffectChosen;
            GameMultiplayer.Instance.OnLastManStanding += Game_OnLastManStanding;
            PlayerActions.Instance.OnChooseCardEffectBegin += Player_OnChooseCardEffectBegin;
        }
    }

    private void Game_OnLastManStanding(object sender, GameMultiplayer.OnLastManStandingEventArgs e)
    {
        state.Value = State.FINISHED;
    }

    private void Game_OnCardEffectChosen(object sender, EventArgs e)
    {
        chooseCardTimer.Value = 0f;
    }

    private void Player_OnChooseCardEffectBegin(object sender, EventArgs e)
    {
        state.Value = State.X_CARD_CHOOSING_CARD_STATE;
    }

    private void Game_OnTrollChoseCard(object sender, EventArgs e)
    {
        trollTimer.Value = 0f;
    }

    private void Game_OnSomeoneTrolled(object sender, GameMultiplayer.OnSomeoneTrolledEventArgs e)
    {
        state.Value = State.TROLL_CARD_STATE;
    }

    private void Game_OnSomeoneTurnStolen(object sender, GameMultiplayer.OnSomeoneTurnStolenEventArgs e)
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
        {
            if (NetworkManager.Singleton.ConnectedClientsIds[i] == e.thief_id)
            {
                connectedClientIdTurnIndex.Value = i;
            }
        }
    }

    private void Game_OnPlayerSelectingCardsFinished(object sender, GameMultiplayer.OnPlayerSelectingCardsFinishedEventArgs e)
    {
        //Debug.Log("player has selected their cards");

        sameCards = e.isMatching;

        // easy way to force turn to finish (should be)
        if (IsServer)
        {
            playerTurnTimer.Value = 0f;
        }
    }

    private void Player_OnInteractPerformed(object sender, EventArgs e)
    {
        if (state.Value == State.WAITING_TO_START && !isLocalPlayerReady)
        {
            isLocalPlayerReady = true;
            OnLocalPlayerReady?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();
        }
    }

    private void SetCardGenerators()
    {
        cardGenerator[0] += () => {
            return new AxeCard();
        };
        cardGenerator[1] += () => {
            return new ForceShuffleCard();
        };
        cardGenerator[2] += () => {
            return new LifeGainCard();
        };
        cardGenerator[3] += () => {
            return new LifeStealCard();
        };
        cardGenerator[4] += () => {
            return new MineCard();
        };
        cardGenerator[5] += () => {
            return new TrollCard();
        };
        cardGenerator[6] += () => {
            return new TurnStealCard();
        };
        cardGenerator[7] += () => {
            return new XCard();
        };
    }
    public Sprite GetCardSpriteWithIndex(int i)
    {
        return cardSpriteArray[i];
    }

    public void AssignCards(List<CardSpot> cardSpots)
    {
        Dictionary<int, int> deckBreakdown = new Dictionary<int, int>();

        deckBreakdown.Add(AXE_CARD_SPRITE_INDEX, 4);
        deckBreakdown.Add(FORCE_SHUFFLE_CARD_SPRTE_INDEX, 4);
        deckBreakdown.Add(LIFE_GAIN_CARD_SPRITE_INDEX, 4);
        deckBreakdown.Add(LIFE_STEAL_CARD_SPRITE_INDEX, 4);
        deckBreakdown.Add(MINE_CARD_SPRTE_INDEX, 8);
        deckBreakdown.Add(TROLL_CARD_SPRITE_INDEX, 4);
        deckBreakdown.Add(TURN_STEAL_CARD_SPRITE_INDEX, 6);
        deckBreakdown.Add(X_CARD_SPRITE_INDEX, 2);

        int prevIndex = -1;

        foreach (CardSpot cardSpot in cardSpots)
        {
            
            int cardIndex = UnityEngine.Random.Range(AXE_CARD_SPRITE_INDEX, X_CARD_SPRITE_INDEX + 1);
            
            //Debug.Log("cardIndex: " + cardIndex);

            if (deckBreakdown[cardIndex] != 0 && (prevIndex != -1 && cardIndex != prevIndex))
            {
                deckBreakdown[cardIndex]--;
                cardSpot.SetCard(cardGenerator[cardIndex - 1]());
            }
            else
            {
                if (cardIndex == prevIndex)
                {
                    cardIndex++;
                    if (cardIndex == 9)
                    {
                        cardIndex = 1;
                    }
                }

                int start = cardIndex;

                while (deckBreakdown[cardIndex] == 0)
                {
                    cardIndex++;
                    if (cardIndex == 9)
                    {
                        cardIndex = 1;
                    }

                    if (cardIndex == start)
                    {
                        return;
                    }
                }

                deckBreakdown[cardIndex]--;
                cardSpot.SetCard(cardGenerator[cardIndex - 1]());

            }

            prevIndex = cardIndex;
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        if (AllPlayersReady())
        {
            state.Value = State.PLAYER_TURN;

            ulong turnClientId = NetworkManager.Singleton.ConnectedClientsIds[connectedClientIdTurnIndex.Value];

            GameMultiplayer.Instance.SetPlayerTurnWithClientId(turnClientId, true);

            OnNextTurnEventCallClientRpc();
        }
    }

    [ClientRpc]
    private void OnNextTurnEventCallClientRpc()
    {
        // Debug.Log("clientId with turn: " + NetworkManager.Singleton.ConnectedClientsIds[connectedClientIdTurnIndex.Value]);
        OnNextTurn?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc (RequireOwnership = false)]
    private void OnGoldenRoundActivatedEventCallServerRpc()
    {
        OnGoldenRoundActivatedEventCallClientRpc();
    }

    [ClientRpc]
    private void OnGoldenRoundActivatedEventCallClientRpc()
    {
        OnGoldenRoundActivated?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    private void OnPlayerChoosingAnotherPlayerEventCallClientRpc()
    {
        // Debug.Log("clientId with turn: " + NetworkManager.Singleton.ConnectedClientsIds[connectedClientIdTurnIndex.Value]);
        OnPlayerChoosingAnotherPlayer?.Invoke(this, EventArgs.Empty);
    }

    // Only serverRpc calls this
    private bool AllPlayersReady()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log("this clientId: " + clientId);
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                return false;
            }
        }

        return true;
    }

    public bool IsPlayerTurn()
    {
        return state.Value == State.PLAYER_TURN;
    }

    public bool InTrollState()
    {
        return state.Value == State.TROLL_CARD_STATE;
    }

    public ICard GenerateCardWithIndex(int index)
    {
        return cardGenerator[index]();
    }

    public void ShuffleCards()
    {
        // Ask grid on server to shuffle cards, then set client grid's cards based off server grid
        Grid.Instance.ShuffleGrid();
    }

    public int GetCurrentPlayerDataTurnIndex()
    {
        return connectedClientIdTurnIndex.Value;
    }

    public bool IsGameFinished()
    {
        return state.Value == State.FINISHED;
    }

    public void SetChosenPlayerIndex(int index)
    {
        SetChosenPlayerServerRpc(index);
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetChosenPlayerServerRpc(int index)
    {
        chosenPlayerIndex = index;
        playerChooseOtherPlayerTimer.Value = 0;
    }

    public void UpdateGraveyard(int index)
    {
        UpdateGraveyardServerRpc(index);
    }

    [ServerRpc (RequireOwnership = false)]
    private void UpdateGraveyardServerRpc(int index)
    {
        UpdateGraveyardClientRpc(index);
    }

    [ClientRpc]
    private void UpdateGraveyardClientRpc(int index)
    {
        if (graveyard[index] == null)
        {
            graveyard[index] = GenerateCardWithIndex(index);
        }
    }

    public ICard[] GetGraveyard()
    {
        return graveyard;
    }

    public void ResetGameState()
    {
        ResetGameStateServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    private void ResetGameStateServerRpc()
    {
        state.Value = State.PLAYER_TURN;
        cardsUsed = 0;
    }

    private void OnDestroy()
    {
        if (NetworkManager.LocalClientId == NetworkManager.ServerClientId)
        {
            GameMultiplayer.Instance.OnPlayerSelectingCardsFinished -= Game_OnPlayerSelectingCardsFinished;
            GameMultiplayer.Instance.OnSomeoneTurnStolen -= Game_OnSomeoneTurnStolen;
            GameMultiplayer.Instance.OnSomeoneTrolled -= Game_OnSomeoneTrolled;
            GameMultiplayer.Instance.OnTrollChoseCard -= Game_OnTrollChoseCard;
            GameMultiplayer.Instance.OnCardEffectChosen -= Game_OnCardEffectChosen;
            GameMultiplayer.Instance.OnLastManStanding -= Game_OnLastManStanding;
        }
    }
}
