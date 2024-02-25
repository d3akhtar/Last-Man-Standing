using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnStealCard : _BaseCard, ICard
{
    public override void CardEffect(int mainPlayerIndex, int chosenPlayerIndex)
    {
        // Steal the next player's turn
        PlayerActions.Instance.StealTurn(mainPlayerIndex, chosenPlayerIndex);

        GameManager.Instance.UpdateGraveyard(GetCardGeneratorIndex());
    }

    public override string GetCardType()
    {
        return "Turn Steal";
    }

    public override Sprite GetCardSprite()
    {
        return GameManager.Instance.GetCardSpriteWithIndex(GameManager.TURN_STEAL_CARD_SPRITE_INDEX);
    }

    public override int GetCardGeneratorIndex()
    {
        return GameManager.TURN_STEAL_CARD_SPRITE_INDEX - 1;
    }
    public override bool RequiresChoosingPlayer()
    {
        return true;
    }
}
