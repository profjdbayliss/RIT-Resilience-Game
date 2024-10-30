using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreManager {

    private class ScoreAmounts {

        public static readonly int VictoryBonus = 100;
        // Blue team scores
        public static readonly int OperationalCoreSector = 20;
        public static readonly int OperationalSector = 10;
        public static readonly int DoomClockPrevention = 15;

        // Red team scores
        public static readonly int DownedCoreSector = 20;
        public static readonly int DownedSector = 10;
        public static readonly int DoomClockActivation = 15;

        // Blue players
        public static readonly int FacilityPreservation = 5;
        public static readonly int CoreFacilitySupport = 5;
        public static readonly int FacilityRestoration = 3;
        public static readonly int ResistanceRestoration = 1;

        public static readonly int MeeplesSpent = 1;
        public static readonly int MeepleSharing = 2;

        public static readonly int FacilityFortification = 2;
        public static readonly int BackdoorRemoval = 3;
        public static readonly int SuccessfulDefenseCards = 2;

        // Red players
        public static readonly int FacilityTakeDown = 5;
        public static readonly int CoreFacilitySabotage = 7;
        public static readonly int ResistanceReduction = 1;

        public static readonly int BackdoorInstallation = 3;
        public static readonly int MeeplesSpentRed = 1;
        public static readonly int ColorlessMeepleUsage = 2;

        public static readonly int PersistentEffects = 2;
        public static readonly int OvercomingFortifications = 2;
        public static readonly int DoomClockManipulation = 5;
    }




    private static readonly ScoreManager instance = new ScoreManager();

    private ScoreManager() {
        teamScores = new Dictionary<PlayerTeam, int>();
        playerScores = new Dictionary<int, int>();
    }

    public static ScoreManager Instance {
        get {
            return instance;
        }
    }

    private Dictionary<PlayerTeam, int> teamScores;
    private Dictionary<int, int> playerScores;

    #region End Game Scoring Functions
    public void CheckUpSectors() {

        GameManager.Instance.AllSectors.ToList().ForEach(sector => {
            AddTeamScore(PlayerTeam.Blue, sector.Value.IsDown ?
                0 : sector.Value.isCore ?
                ScoreAmounts.OperationalCoreSector : ScoreAmounts.OperationalSector);

            AddTeamScore(PlayerTeam.Red, sector.Value.IsDown ?
                sector.Value.isCore ?
                ScoreAmounts.DownedCoreSector : ScoreAmounts.DownedSector : 0);
        });
    }
    public void CheckFacilityStatus() {
        GameManager.Instance.AllSectors.Values
                            .Where(sector => sector.Owner != null).ToList().ForEach(
                            sector => {
                                foreach (var facility in sector.facilities) {
                                    if (facility.WasEverDowned) continue;
                                    AddPlayerScore(sector.Owner.NetID, ScoreAmounts.FacilityPreservation);
                                }
                                
                            }); 
    }


    public void AddEndgameScore() {
        AddTeamScore(GameManager.Instance.GetTurnsLeft() == 0 ? PlayerTeam.Blue : PlayerTeam.Red,
            ScoreAmounts.VictoryBonus);
        CheckUpSectors();


    }
    #endregion

    // Team Scoring Methods
    #region Team Scoring

    public void AddDoomClockPrevention() {
        AddTeamScore(PlayerTeam.Blue, ScoreAmounts.DoomClockPrevention);
    }

    // Red Team Scoring
    public void AddDoomClockActivation() {
        AddTeamScore(PlayerTeam.Red, ScoreAmounts.DoomClockActivation);
    }

    private void AddTeamScore(PlayerTeam team, int points) {
        if (!teamScores.ContainsKey(team)) {
            teamScores[team] = 0;
        }
        teamScores[team] += points;
    }

    public int GetTeamScore(PlayerTeam team) {
        if (!teamScores.ContainsKey(team)) {
            return 0;
        }
        return teamScores[team];
    }

    #endregion

    // Individual Scoring Methods
    #region Individual Scoring

    #region Blue Players
    public void AddCoreFacilitySupport(int playerId) {
        AddPlayerScore(playerId, ScoreAmounts.CoreFacilitySupport);
    }
    public void AddFacilityRestoration(int playerId) {
        AddPlayerScore(playerId, ScoreAmounts.FacilityRestoration);
    }
    public void AddResistancePointsRestored(int playerId, int points) {
        AddPlayerScore(playerId, points * ScoreAmounts.ResistanceRestoration);
    }
    public void AddMeeplesSpent(int playerId, int numSpent) {
        AddPlayerScore(playerId, numSpent * ScoreAmounts.MeeplesSpent);
    }
    public void AddMeepleShare(int playerId) {
        AddPlayerScore(playerId, ScoreAmounts.MeepleSharing);
    }
    public void AddFortification(int playerId) {
        AddPlayerScore(playerId, ScoreAmounts.FacilityFortification);
    }
    public void AddBackdoorRemoval(int playerId) {
        AddPlayerScore(playerId, ScoreAmounts.BackdoorRemoval);
    }
    public void AddSuccessfulDefense(int playerId) {
        AddPlayerScore(playerId, ScoreAmounts.SuccessfulDefenseCards);
    }
    #endregion

    #region Red Players
    public void AddFacilityTakeDown(int playerId) {
        AddPlayerScore(playerId, ScoreAmounts.FacilityTakeDown);
    }
    public void AddCoreFacilityTakeDown(int playerId) {
        AddPlayerScore(playerId, ScoreAmounts.CoreFacilitySabotage);
    }
    public void AddBackdoorCreation(int playerId) {
        AddPlayerScore(playerId, ScoreAmounts.BackdoorInstallation);
    }
    public void AddColorlessMeepleSpent(int playerId, int amount) {
        AddPlayerScore(playerId, amount * ScoreAmounts.ColorlessMeepleUsage);
    }
    public void AddEffectExpireScore(int playerId) {
        AddPlayerScore(playerId, ScoreAmounts.PersistentEffects);
    }
    public void AddFortifyOvercome(int playerId) {
        AddPlayerScore(playerId, ScoreAmounts.OvercomingFortifications);
    }
    public void AddDoomClockActivateionPersonal(int playerId) {
        AddPlayerScore(playerId, ScoreAmounts.DoomClockManipulation);
    }
    #endregion

    private void AddPlayerScore(int playerId, int points) {
        if (!playerScores.ContainsKey(playerId)) {
            playerScores[playerId] = 0;
        }
        Debug.Log($"_score_ Adding {points} to {GameManager.Instance.playerDictionary[playerId].playerName}");
        playerScores[playerId] += points;
    }

    public int GetPlayerScore(int playerId) {
        if (!playerScores.ContainsKey(playerId)) {
            return 0;
        }
        return playerScores[playerId];
    }

    #endregion
}
