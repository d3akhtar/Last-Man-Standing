using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineCard : _BaseCard, ICard
{
    public override void CardEffect(int mainPlayerIndex, int chosenPlayerIndex)
    {
        // Take one life from player
        PlayerActions.Instance.SubtractLife(mainPlayerIndex);

        GameManager.Instance.UpdateGraveyard(GetCardGeneratorIndex());
    }

    public override string GetCardType()
    {
        return "Mine";
    }

    public override Sprite GetCardSprite()
    {
        return GameManager.Instance.GetCardSpriteWithIndex(GameManager.MINE_CARD_SPRTE_INDEX);
    }

    public override int GetCardGeneratorIndex()
    {
        return GameManager.MINE_CARD_SPRTE_INDEX - 1;
    }

    public override bool RequiresChoosingPlayer()
    {
        return false;
    }
}
