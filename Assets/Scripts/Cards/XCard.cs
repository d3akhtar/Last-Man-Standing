using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XCard : _BaseCard, ICard
{
    public override void CardEffect(int mainPlayerIndex, int chosenPlayerIndex)
    {
        // Wildcard
        PlayerActions.Instance.ChooseCardEffectOutOfGraveyard();
    }

    public override string GetCardType()
    {
        return "X";
    }

    public override Sprite GetCardSprite()
    {
        return GameManager.Instance.GetCardSpriteWithIndex(GameManager.X_CARD_SPRITE_INDEX);
    }

    public override int GetCardGeneratorIndex()
    {
        return GameManager.X_CARD_SPRITE_INDEX - 1;
    }

    public override bool RequiresChoosingPlayer()
    {
        return false;
    }

}
