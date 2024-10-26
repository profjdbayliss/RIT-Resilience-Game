using System.Collections.Generic;
using System.Linq;

public class ScoreManager {

    private class ScoreAmounts {
        //blue team scores
        public static readonly int VictoryBonus = 100;
        public static readonly int OperationalSector = 10;
        public static readonly int OperationalCoreSector = 20;
        public static readonly int DoomClockAvoidance = 15;

        //red team scores
        public static readonly int DownedSector = 10;
        public static readonly int DownedCoreSector = 20;
        public static readonly int DoomClockActivation = 15;

        //blue players
        public static readonly int FacilityPreservation = 5;
        public static readonly int FacilityRestoration = 3;
        public static readonly int CoreFacilityDefense = 5;
        public static readonly int ResistancePointsRestored = 1;

        public static readonly int MeeplesSpent = 1;
        public static readonly int MeepleSharing = 2;

        public static readonly int SuccessfulDefenseCards = 2;
        public static readonly int FacilityFortification = 2;
        public static readonly int PreventingBackdoors = 3;

        public static readonly int FacilityLoss = -3;
        public static readonly int CoreSectorBreach = -5;

        //red players
        public static readonly int FacilityTakeDown = 5;
        public static readonly int CoreFacilitySabotage = 7;
        public static readonly int ResistancePointsReduced = 1;

        public static readonly int RedTeamMeeplesSpent = 1;
        public static readonly int ColorlessMeepleUsage = 2;
        public static readonly int BackdoorInstallation = 3;
        public static readonly int PersistentEffects = 2;
        public static readonly int OvercomingFortifications = 2;
        public static readonly int FailedAttacks = -2;
        public static readonly int BackdoorRemoval = -3;
        public static readonly int MeepleWaste = -1;
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
                                AddPlayerScore(sector.Owner.NetID, ScoreAmounts.FacilityPreservation);
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

    public void AddDoomClockAvoidance() {
        AddTeamScore(PlayerTeam.Blue, ScoreAmounts.DoomClockAvoidance);
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

    // Blue Team Players
    // Facility Defense and Restoration
    public void FacilityPreservation(int playerId, int count) {
        AddPlayerScore(playerId, count * 5);
    }

    public void FacilityRestoration(int playerId, int count) {
        AddPlayerScore(playerId, count * 3);
    }

    public void CoreFacilityDefense(int playerId, int count) {
        AddPlayerScore(playerId, count * 5);
    }

    public void ResistancePointsRestored(int playerId, int count) {
        AddPlayerScore(playerId, count * 1);
    }

    // Meeple Management
    public void MeeplesSpent(int playerId, int count) {
        AddPlayerScore(playerId, count * 1);
    }

    public void MeepleSharing(int playerId, int count) {
        AddPlayerScore(playerId, count * 2);
    }

    // Card Play and Strategy
    public void SuccessfulDefenseCards(int playerId, int count) {
        AddPlayerScore(playerId, count * 2);
    }

    public void FacilityFortification(int playerId, int count) {
        AddPlayerScore(playerId, count * 2);
    }

    public void PreventingBackdoors(int playerId, int count) {
        AddPlayerScore(playerId, count * 3);
    }

    // Penalties
    public void FacilityLoss(int playerId, int count) {
        AddPlayerScore(playerId, count * -3);
    }

    public void CoreSectorBreach(int playerId, int count) {
        AddPlayerScore(playerId, count * -5);
    }

    // Red Team Players
    // Facility Sabotage
    public void FacilityTakeDown(int playerId, int count) {
        AddPlayerScore(playerId, count * 5);
    }

    public void CoreFacilitySabotage(int playerId, int count) {
        AddPlayerScore(playerId, count * 7);
    }

    public void ResistancePointsReduced(int playerId, int count) {
        AddPlayerScore(playerId, count * 1);
    }

    // Meeple Utilization
    public void RedTeamMeeplesSpent(int playerId, int count) {
        AddPlayerScore(playerId, count * 1);
    }

    public void ColorlessMeepleUsage(int playerId, int count) {
        AddPlayerScore(playerId, count * 2);
    }

    // Card Play and Strategy
    public void BackdoorInstallation(int playerId, int count) {
        AddPlayerScore(playerId, count * 3);
    }

    public void PersistentEffects(int playerId, int count) {
        AddPlayerScore(playerId, count * 2);
    }

    public void OvercomingFortifications(int playerId, int count) {
        AddPlayerScore(playerId, count * 2);
    }

    // Penalties
    public void FailedAttacks(int playerId, int count) {
        AddPlayerScore(playerId, count * -2);
    }

    public void BackdoorRemoval(int playerId, int count) {
        AddPlayerScore(playerId, count * -3);
    }

    public void MeepleWaste(int playerId, int count) {
        AddPlayerScore(playerId, count * -1);
    }

    private void AddPlayerScore(int playerId, int points) {
        if (!playerScores.ContainsKey(playerId)) {
            playerScores[playerId] = 0;
        }
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
