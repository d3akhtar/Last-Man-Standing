using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class _BaseCard : MonoBehaviour, ICard
{
    public virtual void CardEffect(int mainPlayerIndex, int chosenPlayerIndex)
    {
        Debug.Log("Base CardEffect() called");
    }

    public virtual Sprite GetCardSprite()
    {
        Debug.Log("Base GetCardSprite() called");
        return null;
    }

    public virtual string GetCardType()
    {
        Debug.Log("Base GetCardType() called");
        return null;
    }

    public virtual Sprite GetBackOfCardSprite()
    {
        return GameManager.Instance.GetCardSpriteWithIndex(GameManager.BACK_OF_CARD_SPRITE_INDEX);
    }

    public virtual int GetCardGeneratorIndex()
    {
        return -1;
    }

    public virtual bool RequiresChoosingPlayer()
    {
        return false;
    }
}
