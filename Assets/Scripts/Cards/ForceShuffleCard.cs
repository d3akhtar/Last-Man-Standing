using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceShuffleCard : _BaseCard, ICard
{
    public override void CardEffect(int mainPlayerIndex, int chosenPlayerIndex)
    {
        // Shuffle deck
        GameManager.Instance.ShuffleCards();

        GameManager.Instance.UpdateGraveyard(GetCardGeneratorIndex());
    }

    public override string GetCardType()
    {
        return "Force Shuffle";
    }

    public override Sprite GetCardSprite()
    {
        return GameManager.Instance.GetCardSpriteWithIndex(GameManager.FORCE_SHUFFLE_CARD_SPRTE_INDEX);
    }

    public override int GetCardGeneratorIndex()
    {
        return GameManager.FORCE_SHUFFLE_CARD_SPRTE_INDEX - 1;
    }

    public override bool RequiresChoosingPlayer()
    {
        return false;
    }
}
