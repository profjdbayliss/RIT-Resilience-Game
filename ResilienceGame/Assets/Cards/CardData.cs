using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Card;

public enum CardType
{
    Station,
    Defense,
    Vulnerability,
    Mitigation,
    Instant,
    Special,
    None

    // TODO: Add Card Types for SD
}

public struct CardData
{
    public int numberInDeck;
    public CardType cardType;
    public float percentSuccess;
    public int cardID;
    public int teamID; // TODO: Use for SD
    public int blueCost;
    public int blackCost;
    public int purpleCost;
    public int drawAmount;
    public int removeAmount;
    public int targetAmount;
    public int facilityAmount;
    public FacilityEffect effect;
    public FacilityEffect preReqEffect;
    public int effectCount;
    public int duration;
    public bool hasDoomEffect;
    public string[] meepleType;
    public float meepleAmount;
    public PlayerSector[] onlyPlayedOn;
}
