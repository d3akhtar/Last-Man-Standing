using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeGainCard : _BaseCard, ICard
{
    public override void CardEffect(int mainPlayerIndex, int chosenPlayerIndex)
    {
        // Give player a life
        PlayerActions.Instance.AddLife(mainPlayerIndex);

        GameManager.Instance.UpdateGraveyard(GetCardGeneratorIndex());
    }

    public override string GetCardType()
    {
        return "Life Gain";
    }

    public override Sprite GetCardSprite()
    {
        return GameManager.Instance.GetCardSpriteWithIndex(GameManager.LIFE_GAIN_CARD_SPRITE_INDEX);
    }

    public override int GetCardGeneratorIndex()
    {
        return GameManager.LIFE_GAIN_CARD_SPRITE_INDEX - 1;
    }

    public override bool RequiresChoosingPlayer()
    {
        return false;
    }

}
