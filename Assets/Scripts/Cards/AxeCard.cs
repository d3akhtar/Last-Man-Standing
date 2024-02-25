using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeCard : _BaseCard, ICard
{
    public override void CardEffect(int mainPlayerIndex, int chosenPlayerIndex)
    {
        PlayerActions.Instance.SubtractLife(chosenPlayerIndex);

        GameManager.Instance.UpdateGraveyard(GetCardGeneratorIndex());
    }

    public override string GetCardType()
    {
        return "Axe";
    }

    public override Sprite GetCardSprite()
    {
        return GameManager.Instance.GetCardSpriteWithIndex(GameManager.AXE_CARD_SPRITE_INDEX);
    }

    public override int GetCardGeneratorIndex()
    {
        return (GameManager.AXE_CARD_SPRITE_INDEX) - 1;
    }

    public override bool RequiresChoosingPlayer()
    {
        return true;
    }
}
