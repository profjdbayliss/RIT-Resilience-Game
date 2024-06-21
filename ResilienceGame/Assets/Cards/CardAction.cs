using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAction : MonoBehaviour
{
    public enum ActionType {
        DrawAndDiscardCards, // [Draw Count] [Discard Count]
        ShuffleAndDrawCards, // [Shuffle Count] [Draw Count]
        ChangeNetworkPoints, // [Value Change]
        ChangePhysicalPoints, // [Value Change]
        ChangeFinancialPoints, // [Value Change]
        AddEffect, // [Effect Type]
        RemoveEffect, // [Effect Type]
        NegatePointsReductions, //
        NegateEffect, // [Effect Type]
        AddSpecifyCardsFromDeck, // [Card Name]
        ShowEffectsOnFacilities //
    }

    public ActionType type; 
    public List<string> parameters = new List<string>();
    public List<Facility> targetFacilities = new List<Facility>();

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ExecuteAction()
    {

    }
}
