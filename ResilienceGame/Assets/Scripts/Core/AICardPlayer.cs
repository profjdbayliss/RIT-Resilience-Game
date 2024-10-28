using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class AICardPlayer : MonoBehaviour {
    CardPlayer cardPlayer;


    List<Facility> DamagedFacilities => cardPlayer.PlayerSector.facilities.Where(facility => facility.IsDamaged).ToList();
    // Start is called before the first frame update
    void Start() {
        cardPlayer = GetComponent<CardPlayer>();
    }

    // Update is called once per frame
    void Update() {
        if (Keyboard.current.numpad0Key.wasPressedThisFrame) {
            PlayCard();
        }
    }
    public void PlayCard() {
        Debug.Log($"AI on player {cardPlayer.playerName} is playing a card");
        if (cardPlayer.playerTeam != PlayerTeam.Blue) return;

        var cardToPlay = GetRandomPlayableCard(
            cardPlayer.HandCards.Values.Select(x => x.GetComponent<Card>()).ToList(),
            out GameObject playLocation);

        Debug.Log($"Found card able to be played: {cardToPlay.data.name}");

        if (cardToPlay == null) return;
        if (playLocation == null) return;

        cardPlayer.AiDropCardOn(cardToPlay, playLocation.transform.position);

       
    }
    private Card GetRandomPlayableCard(List<Card> playerHand, out GameObject playLocation) {

        if (!playerHand.Any()) {
            playLocation = null;
            return null;

        }

        Card cardToPlay = null;
        //get random card in the hand
        cardToPlay = playerHand[Random.Range(0, playerHand.Count)];

        //check the card target
        switch (cardToPlay.target) {
            case CardTarget.Hand:
            case CardTarget.Card:
            case CardTarget.Sector:
                playLocation = cardPlayer.PlayerSector.facilities[1].gameObject;
                return cardToPlay;
            case CardTarget.Effect:
            case CardTarget.Facility:
                switch (cardToPlay.ActionList[0]) {
                    case AddEffect:
                        var facilityToPlayOn = GetValidFacilityToPlayOn(cardToPlay);
                        if (facilityToPlayOn != null) {
                            playLocation = facilityToPlayOn.gameObject;
                            return cardToPlay;
                        }
                        break;
                    case BackdoorCheckNetworkRestore:
                        playLocation = cardPlayer.PlayerSector.facilities[1].gameObject;
                        return cardToPlay;
                }
                break;
        }
        playerHand.Remove(cardToPlay);
        return GetRandomPlayableCard(playerHand, out playLocation);


    }

    private Facility GetRestorableFacility(FacilityEffect cardEffect) {
        if (cardEffect.EffectType != FacilityEffectType.ModifyPoints) return null;
        foreach (var facility in DamagedFacilities) {
            switch (cardEffect.Target) {
                case FacilityEffectTarget.Physical:
                    if (!facility.HasMaxPhysicalPoints) return facility;
                    break;
                case FacilityEffectTarget.Financial:
                    if (!facility.HasMaxFinancialPoints) return facility;
                    break;
                case FacilityEffectTarget.Network:
                    if (!facility.HasMaxNetworkPoints) return facility;
                    break;
                case FacilityEffectTarget.NetworkPhysical:
                    if (!facility.HasMaxNetworkPoints || !facility.HasMaxPhysicalPoints) return facility;
                    break;
                case FacilityEffectTarget.FinancialNetwork:
                    if (!facility.HasMaxNetworkPoints || !facility.HasMaxFinancialPoints) return facility;
                    break;
                case FacilityEffectTarget.FinancialPhysical:
                    if (!facility.HasMaxPhysicalPoints || !facility.HasMaxFinancialPoints) return facility;
                    break;

            }
        }
        return null;
    }
    private Facility GetFortifiableFacility(FacilityEffect cardEffect) {
        foreach (var facility in cardPlayer.PlayerSector.facilities) {
            if (!facility.IsFortified()) {
                return facility;
            }
        }
        return cardPlayer.PlayerSector.facilities[0];
    }
    private Facility GetFacilityWithRemovableEffects(Card card) {
        if (cardPlayer.PlayerSector.GetFacilityWithRemovableEffects(PlayerTeam.Blue, out Facility facility)) {
            return facility;
        }
        return null;
    }
    private Facility GetValidFacilityToPlayOn(Card card) {
        var cardEffect = FacilityEffect.CreateEffectsFromID(card.data.effectString)[0];
        if (cardEffect == null) return null;
        switch (cardEffect.EffectType) {
            case FacilityEffectType.ModifyPoints:
                return GetRestorableFacility(cardEffect);
            case FacilityEffectType.Fortify:
                return GetFortifiableFacility(cardEffect);
            case FacilityEffectType.RemoveAll:
            case FacilityEffectType.RemoveOne:
                return GetFacilityWithRemovableEffects(card);
            default:
                return cardPlayer.PlayerSector.facilities[1]; //return middle facility by default for now

        }
    }


}
