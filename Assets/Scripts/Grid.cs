using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private List<CardSpot> cardSpots;
    private void Awake()
    {
        cardSpots = new List<CardSpot>();
    }

    public List<CardSpot> GetCardSpots()
    {
        return cardSpots;
    }
}
