using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface ICard
{
    public abstract void CardEffect(int mainPlayerIndex, int chosenPlayerIndex);

    public abstract string GetCardType();

    public abstract Sprite GetCardSprite();

    public abstract Sprite GetBackOfCardSprite();

    public abstract int GetCardGeneratorIndex();

    public abstract bool RequiresChoosingPlayer();

}
