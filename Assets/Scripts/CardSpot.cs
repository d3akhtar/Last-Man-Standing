using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TerrainUtils;

public class CardSpot : NetworkBehaviour
{
    [SerializeField] private GameObject outlineVisualGameObject;

    private BoxCollider2D selectCollider;

    private GameObject cardSpriteGameObject;
    private ICard card;

    private bool isSelected = false;

    private bool firstColliderEnableOccured = false;

    private Color selectedColor = Color.black;
    private Color unselectedColor = Color.gray;
    private Color trollSelectedColor = Color.red;

    private int indexInCardSpotList;

    public event EventHandler<OnThisCardSelectedEventArgs> OnThisCardSelected;
    public event EventHandler<OnThisCardSelectedEventArgs> OnTrollSelectedThisCard;

    private bool isDisabled = false;
    public class OnThisCardSelectedEventArgs : EventArgs
    {
        public ICard card;
        public int cardSpotIndex;
    }

    private void Awake()
    {
        selectCollider = GetComponent<BoxCollider2D>();

        outlineVisualGameObject.GetComponent<SpriteRenderer>().color = unselectedColor;
        outlineVisualGameObject.SetActive(false);
    }

    private void Start()
    {
        selectCollider.enabled = false; // only enable when game starts

        GameManager.Instance.OnStateChanged += Game_OnStateChanged;
        GameManager.Instance.OnCardsMatching += Game_OnCardsMatching;
        GameManager.Instance.OnCardsNotMatching += Game_OnCardsNotMatching;
    }

    private void Game_OnCardsNotMatching(object sender, EventArgs e)
    {
        ShowBackOfCardServerRpc();
    }

    private void Game_OnCardsMatching(object sender, EventArgs e)
    {
        if (isSelected)
        {
            DisableCardSpotServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisableCardSpotServerRpc()
    {
        Debug.Log("calling DisableCardSpotClientRpc");
        DisableCardSpotClientRpc();
        Debug.Log("done DisableCardSpotClientRpc");
    }

    [ClientRpc]
    private void DisableCardSpotClientRpc()
    {
        DisableCardSpot();
    }

    private void DisableCardSpot()
    {
        cardSpriteGameObject.SetActive(false);
        selectCollider.enabled = false;
        outlineVisualGameObject.SetActive(false);
        outlineVisualGameObject.GetComponent<SpriteRenderer>().color = unselectedColor;
        isDisabled = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void EnableCardSpotServerRpc()
    {
        EnableCardSpotClientRpc();
    }

    [ClientRpc]
    private void EnableCardSpotClientRpc()
    {
        EnableCardSpot();
    }

    private void EnableCardSpot()
    {
        selectCollider.enabled = true;
        outlineVisualGameObject.SetActive(false);
        outlineVisualGameObject.GetComponent<SpriteRenderer>().color = unselectedColor;
        isDisabled = false;
    }

    private void Game_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsPlayerTurn() && !firstColliderEnableOccured)
        {
            firstColliderEnableOccured = true;
            selectCollider.enabled = true;
        }
    }

    private void OnMouseOver()
    {
        if (card != null)
        {
            if (GameMultiplayer.Instance.IsLocalClientTurn() && GameManager.Instance.IsPlayerTurn())
            {
                outlineVisualGameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.Mouse0) && outlineVisualGameObject.GetComponent<SpriteRenderer>().color == unselectedColor)
                {
                    if (outlineVisualGameObject.GetComponent<SpriteRenderer>().color == unselectedColor)
                    {
                        outlineVisualGameObject.GetComponent<SpriteRenderer>().color = selectedColor;
                        ShowFrontOfCardServerRpc();
                        OnThisCardSelectedEventCallServerRpc();
                    }
                }
            }
            else if (GameMultiplayer.Instance.IsLocalClientTroll() && GameManager.Instance.InTrollState())
            {
                outlineVisualGameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.Mouse0) && outlineVisualGameObject.GetComponent<SpriteRenderer>().color == unselectedColor)
                {
                    if (outlineVisualGameObject.GetComponent<SpriteRenderer>().color == unselectedColor)
                    {
                        outlineVisualGameObject.GetComponent<SpriteRenderer>().color = trollSelectedColor;
                        ShowFrontOfCardServerRpc();
                        OnTrollSelectedThisCardEventCallServerRpc();
                    }
                }
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void OnThisCardSelectedEventCallServerRpc()
    {
        int length = OnThisCardSelected.GetInvocationList().Length;

        // event getting subscribe to twice in GameMultiplayer script for some reason, so getting rid of extra events
        if (length > 1)
        {
            for (int i = 1; i < length; i++)
            {
                OnThisCardSelected -= (System.EventHandler<OnThisCardSelectedEventArgs>)OnThisCardSelected.GetInvocationList()[0];
            }
        }

        OnThisCardSelected?.Invoke(this, new OnThisCardSelectedEventArgs
        {
            card = card,
            cardSpotIndex = indexInCardSpotList,
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnTrollSelectedThisCardEventCallServerRpc()
    {
        int length = OnTrollSelectedThisCard.GetInvocationList().Length;

        // doing it again incase, idk if its necessary, will figure out later
        if (length > 1)
        {
            for (int i = 1; i < length; i++)
            {
                OnTrollSelectedThisCard -= (System.EventHandler<OnThisCardSelectedEventArgs>)OnTrollSelectedThisCard.GetInvocationList()[0];
            }
        }

        OnTrollSelectedThisCard?.Invoke(this, new OnThisCardSelectedEventArgs
        {
            card = card,
            cardSpotIndex = indexInCardSpotList,
        });
    }

    [ServerRpc (RequireOwnership = false)]
    private void ShowFrontOfCardServerRpc()
    {
        ShowFrontOfCardClientRpc();
    }

    [ClientRpc]
    private void ShowFrontOfCardClientRpc()
    {
        cardSpriteGameObject.GetComponent<SpriteRenderer>().sprite = card.GetCardSprite();
        isSelected = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowBackOfCardServerRpc()
    {
        ShowBackOfCardClientRpc();
    }

    [ClientRpc]
    private void ShowBackOfCardClientRpc()
    {
        outlineVisualGameObject.GetComponent<SpriteRenderer>().color = unselectedColor;

        outlineVisualGameObject.SetActive(false);

        cardSpriteGameObject.GetComponent<SpriteRenderer>().sprite = card.GetBackOfCardSprite();
        
        isSelected = false;
    }

    private void OnMouseExit()
    {
        if (!isSelected)
        {
            outlineVisualGameObject.SetActive(false);
        }
    }

    public void SetCard(ICard card)
    {
        this.card = card;

        if (card == GameMultiplayer.DEFAULT_CHOSEN_CARD)
        {
            DisableCardSpot();
        }
        else
        {
            EnableCardSpot();
            SpawnCardLocalCenter();
        }
    }

    public void RemoveCard()
    {
        isSelected = false;
        SetCard(GameMultiplayer.DEFAULT_CHOSEN_CARD);
    }

    private void SpawnCardLocalCenter()
    {
        if (cardSpriteGameObject != null)
        {
            Destroy(cardSpriteGameObject);
        }

        cardSpriteGameObject = new GameObject();
        cardSpriteGameObject.name = "Card sprite";
        cardSpriteGameObject.AddComponent<SpriteRenderer>();
        cardSpriteGameObject.GetComponent<SpriteRenderer>().sprite = card.GetBackOfCardSprite();
        //cardSpriteGameObject.GetComponent<SpriteRenderer>().sprite = card.GetCardSprite();
        cardSpriteGameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;

        cardSpriteGameObject.transform.localScale = new Vector3(1, 0.9087f, 1);
        cardSpriteGameObject.transform.position = transform.TransformPoint(Vector3.zero);

        cardSpriteGameObject.transform.parent = this.transform;
    }

    public ICard GetCard()
    {
        return card;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    public int GetIndexInCardSpotList()
    {
        return indexInCardSpotList;
    }

    public void SetIndexInCardSpotList(int index)
    {
        indexInCardSpotList = index;
    }

    public bool IsCardSpotDisabled()
    {
        return isDisabled;
    }
}
