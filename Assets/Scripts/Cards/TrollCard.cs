using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrollCard : _BaseCard, ICard
{
    public override void CardEffect(int mainPlayerIndex, int chosenPlayerIndex)
    {
        // Choose one of your opponents next cards on their next turn
        PlayerActions.Instance.Troll(mainPlayerIndex, chosenPlayerIndex);

        GameManager.Instance.UpdateGraveyard(GetCardGeneratorIndex());
    }

    public override string GetCardType()
    {
        return "Troll";
    }

    public override Sprite GetCardSprite()
    {
        return GameManager.Instance.GetCardSpriteWithIndex(GameManager.TROLL_CARD_SPRITE_INDEX);
    }

    public override int GetCardGeneratorIndex()
    {
        return GameManager.TROLL_CARD_SPRITE_INDEX - 1;
    }

    public override bool RequiresChoosingPlayer()
    {
        return true;
    }
}
