using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChooseCardUI : MonoBehaviour
{
    [SerializeField] private Transform graveyardCardsContainerTransform;

    private void Awake()
    {
        for (int i = 0; i < graveyardCardsContainerTransform.childCount; i++)
        {
            GameObject chooseCardButtonGameObject = graveyardCardsContainerTransform.GetChild(i).gameObject;
            Debug.Log("current chooseCardButtonGameObject name: " + chooseCardButtonGameObject.name);

            int localIndex = i;
            chooseCardButtonGameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                GameMultiplayer.Instance.SetBothOfPlayerCards(localIndex);
                Hide();
            });
            
            chooseCardButtonGameObject.SetActive(false);
        }
    }
    private void Start()
    {
        PlayerActions.Instance.OnChooseCardEffectBegin += Player_OnChooseCardEffectBegin;

        Hide();
    }

    private void Player_OnChooseCardEffectBegin(object sender, System.EventArgs e)
    {
        if (GameMultiplayer.Instance.IsLocalClientTurn())
        {
            ICard[] graveyard = GameManager.Instance.GetGraveyard();
            for (int i = 0; i < graveyardCardsContainerTransform.childCount; i++)
            {
                if (graveyard[i] != null)
                {
                    graveyardCardsContainerTransform.GetChild(i).gameObject.SetActive(true);
                }
            }
            Show();
        }
    }

    private void Show()
    {
        this.gameObject.SetActive(true);
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
