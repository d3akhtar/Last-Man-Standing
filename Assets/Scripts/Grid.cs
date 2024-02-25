using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Grid : NetworkBehaviour
{

    public static Grid Instance { get; private set; }

    public event EventHandler OnGridFinishedSettingCards;

    private List<CardSpot> cardSpots;

    private bool gridHasBeenSet = false;

    private const int MAX_CARD_COUNT = 36;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (IsServer)
        {
            GameManager.Instance.OnStateChanged += Game_OnStateChanged;
            GameManager.Instance.OnGoldenRoundActivated += Game_OnGoldenRoundActivated;
        }

        cardSpots = new List<CardSpot>();

        int i = 0;

        foreach (Transform child in this.transform)
        {
            CardSpot cardSpot = child.GetComponent<CardSpot>();
            cardSpot.SetIndexInCardSpotList(i);
            cardSpots.Add(cardSpot);

            i++;
        }
    }

    private void Game_OnGoldenRoundActivated(object sender, EventArgs e)
    {
        SetUpGridServerRpc(); // reset the grid when golden round is activated
    }

    private void Game_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsPlayerTurn() && !gridHasBeenSet)
        {
            gridHasBeenSet = true;
            SetUpGridServerRpc();
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetUpGridServerRpc()
    {
        GameManager.Instance.AssignCards(cardSpots);

        for (int i = 0; i < cardSpots.Count; i++)
        {
            AssignCardClientRpc(i, cardSpots[i].GetCard().GetCardGeneratorIndex());
        }

        //CallGridFinishedSettingCardsEventClientRpc();
        OnGridFinishedSettingCards?.Invoke(this, EventArgs.Empty);
    }


    [ClientRpc]
    private void AssignCardClientRpc(int index, int generatorIndex)
    {
        ICard card = GameManager.Instance.GenerateCardWithIndex(generatorIndex);

        cardSpots[index].SetCard(card);
    }

    public void ShuffleGrid()
    {
        ShuffleGridServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    private void ShuffleGridServerRpc()
    {
        // Get all the available cards from the server's grid
        List<ICard> cards = new List<ICard>();

        foreach(CardSpot cardSpot in cardSpots)
        {
            if (!cardSpot.IsCardSpotDisabled())
            {
                cards.Add(cardSpot.GetCard());
            }
        }

        // Clear grid for all clients basically
        ClearCardsClientRpc();

        // Distribute available cards randomly, first setting them to server, then to client
        HashSet<int> usedIndexes = new HashSet<int>();
        foreach (ICard card in cards)
        {
            int randomIndex = UnityEngine.Random.Range(0, MAX_CARD_COUNT);
            while (usedIndexes.Contains(randomIndex))
            {
                randomIndex = UnityEngine.Random.Range(0, MAX_CARD_COUNT);
            }
            cardSpots[randomIndex].SetCard(card);
            usedIndexes.Add(randomIndex);

            AssignCardClientRpc(randomIndex, card.GetCardGeneratorIndex());
        }
        
    }

    [ClientRpc]
    private void ClearCardsClientRpc()
    {
        foreach(CardSpot cardSpot in cardSpots)
        {
            cardSpot.RemoveCard();
        }
    }


    [ClientRpc]
    private void CallGridFinishedSettingCardsEventClientRpc()
    {
        OnGridFinishedSettingCards?.Invoke(this, EventArgs.Empty);
    }

    /*
    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        //Your Function You Want to Call
        GameManager.Instance.AssignCards(cardSpots);
    } */

    public List<CardSpot> GetCardSpots()
    {
        return cardSpots;
    }
}
