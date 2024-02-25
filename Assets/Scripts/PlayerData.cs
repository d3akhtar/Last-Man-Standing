using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong clientId;
    
    public FixedString64Bytes name;
    
    public bool hasTurn;

    public bool turnIsStolen;
    public int playerDataIndexOfThief;

    public bool trolled;
    public int playerDataIndexOfTroll;

    public int livesCount;

    public FixedString64Bytes firstCardType;
    public FixedString64Bytes secondCardType;

    public bool Equals(PlayerData other)
    {
        return 
            this.clientId == other.clientId && this.name == other.name && this.hasTurn == other.hasTurn && 
            this.turnIsStolen == other.turnIsStolen && this.playerDataIndexOfThief == other.playerDataIndexOfThief &&
            this.trolled == other.trolled && this.playerDataIndexOfTroll == other.playerDataIndexOfTroll && 
            this.livesCount == other.livesCount && this.firstCardType == other.firstCardType && 
            this.secondCardType == other.secondCardType;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref livesCount);
        serializer.SerializeValue(ref hasTurn);
        serializer.SerializeValue(ref turnIsStolen);
        serializer.SerializeValue(ref playerDataIndexOfThief);
        serializer.SerializeValue(ref trolled);
        serializer.SerializeValue(ref playerDataIndexOfTroll);
        serializer.SerializeValue(ref firstCardType);
        serializer.SerializeValue(ref secondCardType);
    }
}
