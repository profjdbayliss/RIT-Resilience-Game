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
        AddEffect, // [Effect Type] [Duration]
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

    public void ExecuteAction(CardPlayer player)
    {
        switch (type)
        {
            case ActionType.DrawAndDiscardCards:
                player.DrawCards(int.Parse(parameters[0]));
                player.DiscardCards(int.Parse(parameters[1]));
                break;
            case ActionType.ShuffleAndDrawCards:
                player.ShuffleCards(int.Parse(parameters[0]));
                player.DrawCards(int.Parse(parameters[1]));
                break;
            case ActionType.ChangeNetworkPoints:
                foreach (var facility in targetFacilities)
                    facility.networkPoints += int.Parse(parameters[0]);
                break;
            case ActionType.ChangePhysicalPoints:
                foreach (var facility in targetFacilities)
                    facility.physicalPoints += int.Parse(parameters[0]);
                break;
            case ActionType.ChangeFinancialPoints:
                foreach (var facility in targetFacilities)
                    facility.financialPoints += int.Parse(parameters[0]);
                break;
            case ActionType.AddEffect:
                var newEffect = new Effect { type = parameters[0], duration = int.Parse(parameters[1]) };
                foreach (var facility in targetFacilities)
                    facility.effects.Add(newEffect);
                break;
            case ActionType.RemoveEffect:
                string effectToRemove = parameters[0];
                foreach (var facility in targetFacilities)
                    facility.effects.RemoveAll(e => e.type == effectToRemove);
                break;
            case ActionType.NegatePointsReductions:
                foreach (var facility in targetFacilities)
                    facility.negatePointsReduction = true;
                break;
            case ActionType.NegateEffect:
                string effectToNegate = parameters[0];
                foreach (var facility in targetFacilities)
                    facility.negateEffects.Add(new Effect { type = effectToNegate });
                break;
            case ActionType.AddSpecifyCardsFromDeck:
                player.DrawSpecificCard(parameters[0]);
                break;
            case ActionType.ShowEffectsOnFacilities:
                foreach (var facility in targetFacilities)
                    foreach (var effect in facility.effects)
                        Debug.Log($"{facility.facilityName} has effect {effect.type}");
                break;
            default:
                Debug.LogError("Unsupported action type.");
                break;
        }
    }
}
