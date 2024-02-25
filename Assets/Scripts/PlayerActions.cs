using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerActions : NetworkBehaviour
{
    public static PlayerActions Instance { get; private set; }
    public event EventHandler OnChooseCardEffectBegin;

    private void Awake()
    {
        Instance = this;
    }
    public void AddLife(int playerDataIndex)
    {
        GameMultiplayer.Instance.ReplacePlayerDataUsingIndex("ADD_LIFE", playerDataIndex);
    }

    public void SubtractLife(int playerDataIndex)
    {
        GameMultiplayer.Instance.ReplacePlayerDataUsingIndex("SUBTRACT_LIFE", playerDataIndex);
    }

    public void StealTurn(int playerDataIndex, int chosenPlayerDataIndex)
    {
        GameMultiplayer.Instance.ReplacePlayerDataUsingIndex("STEAL_TURN", chosenPlayerDataIndex, playerDataIndex);
    }

    public void StealLife(int playerDataIndex, int chosenPlayerDataIndex)
    {
        GameMultiplayer.Instance.ReplacePlayerDataUsingIndex("STEAL_LIFE", chosenPlayerDataIndex, playerDataIndex);
    }

    public void Troll(int playerDataIndex, int chosenPlayerDataIndex)
    {
        GameMultiplayer.Instance.ReplacePlayerDataUsingIndex("TROLL", chosenPlayerDataIndex, playerDataIndex);
    }

    public void ChooseCardEffectOutOfGraveyard()
    {
        OnChooseCardEffectBeginEventCallServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    private void OnChooseCardEffectBeginEventCallServerRpc()
    {
        OnChooseCardEffectBeginEventCallClientRpc();
    }

    [ClientRpc]
    private void OnChooseCardEffectBeginEventCallClientRpc()
    {
        OnChooseCardEffectBegin?.Invoke(this, EventArgs.Empty);
    }
}
