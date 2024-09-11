
using UnityEngine;

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
// Enum to track the state of the card
public enum CardState {
    NotInDeck,
    CardInDeck,
    CardDrawn,
    CardDrawnDropped,
    CardInPlay,
    CardNeedsToBeDiscarded,
    CardDiscarded,
};

// Enum to indicate what the card is being played on
public enum CardTarget {
    Hand,
    Card,
    Effect,
    Facility,
    Sector
};
public struct CardIDInfo {
    public int UniqueID;
    public int CardID;
};

public struct FrontData {
    public bool blueCircle;
    public bool blackCircle;
    public bool purpleCircle; // TODO: Needs three, one for each meeple color
    public Color color; // TODO: Change name if needed
    public string title;
    public string description;
    public string flavor;
    //public GameObject innerTexts;
    public Texture2D background;
    public Texture2D img;
}

public struct CardData
{
    public CardTarget playableTarget;
    public FrontData front;
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
    public int meepleAmount;
    public PlayerSector[] onlyPlayedOn;
}
