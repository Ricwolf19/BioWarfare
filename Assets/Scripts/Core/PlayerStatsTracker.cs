using UnityEngine;

namespace BioWarfare.Stats
{
    /// <summary>
    /// Tracks player statistics throughout the game
    /// Singleton pattern for easy access from anywhere
    /// </summary>
    public class PlayerStatsTracker : MonoBehaviour
    {
        private static PlayerStatsTracker instance;
        public static PlayerStatsTracker Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("PlayerStatsTracker");
                    instance = go.AddComponent<PlayerStatsTracker>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Player Info")]
        public string PlayerName = "";

        [Header("Game Stats")]
        public int EnemiesKilled = 0;
        public int ZonesCleansed = 0;
        public float GameStartTime = 0f;
        public float GameEndTime = 0f;

        [Header("Status")]
        public bool IsGameActive = false;
        public bool HasDied = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Auto-start game if testing directly from GameScene
        /// </summary>
        private void Start()
        {
            // If game is being tested directly (not from menu), auto-start with random name
            if (string.IsNullOrEmpty(PlayerName) && !IsGameActive)
            {
                string autoName = GenerateRandomPlayerName();
                Debug.LogWarning($"[PlayerStatsTracker] No player name set! Auto-starting game as '{autoName}' (for testing)");
                StartNewGame(autoName);
            }
        }

        #region Game Flow

        /// <summary>
        /// Start tracking a new game
        /// </summary>
        public void StartNewGame(string playerName)
        {
            PlayerName = string.IsNullOrEmpty(playerName) ? GenerateRandomPlayerName() : playerName;
            EnemiesKilled = 0;
            ZonesCleansed = 0;
            GameStartTime = Time.time;
            GameEndTime = 0f;
            IsGameActive = true;
            HasDied = false;

            Debug.Log($"[PlayerStatsTracker] New game started for: {PlayerName}");
        }

        /// <summary>
        /// End game tracking
        /// </summary>
        public void EndGame(bool died)
        {
            if (!IsGameActive) return;

            GameEndTime = Time.time;
            IsGameActive = false;
            HasDied = died;

            Debug.Log($"[PlayerStatsTracker] Game ended. Stats: Kills={EnemiesKilled}, Zones={ZonesCleansed}, Time={GetGameDuration()}s, Died={died}");
        }

        /// <summary>
        /// Reset all stats for new game
        /// </summary>
        public void ResetStats()
        {
            PlayerName = "";
            EnemiesKilled = 0;
            ZonesCleansed = 0;
            GameStartTime = 0f;
            GameEndTime = 0f;
            IsGameActive = false;
            HasDied = false;

            Debug.Log("[PlayerStatsTracker] Stats reset.");
        }

        #endregion

        #region Stat Tracking

        /// <summary>
        /// Increment enemy kill count
        /// </summary>
        public void AddEnemyKill()
        {
            if (IsGameActive)
            {
                EnemiesKilled++;
                Debug.Log($"[PlayerStatsTracker] Enemy killed. Total: {EnemiesKilled}");
            }
        }

        /// <summary>
        /// Increment zone cleansed count
        /// </summary>
        public void AddZoneCleansed()
        {
            if (IsGameActive)
            {
                ZonesCleansed++;
                Debug.Log($"[PlayerStatsTracker] Zone cleansed. Total: {ZonesCleansed}");
            }
        }

        /// <summary>
        /// Get total game duration in seconds
        /// </summary>
        public float GetGameDuration()
        {
            if (GameStartTime == 0) return 0f;

            float endTime = GameEndTime > 0 ? GameEndTime : Time.time;
            return endTime - GameStartTime;
        }

        /// <summary>
        /// Get formatted game duration (MM:SS)
        /// </summary>
        public string GetFormattedDuration()
        {
            float duration = GetGameDuration();
            int minutes = Mathf.FloorToInt(duration / 60f);
            int seconds = Mathf.FloorToInt(duration % 60f);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        #endregion

        #region Helper Methods

        private string GenerateRandomPlayerName()
        {
            int randomNumber = Random.Range(1000, 9999);
            return $"Player{randomNumber}";
        }

        #endregion

        #region Debug

        [ContextMenu("Print Stats")]
        public void PrintStats()
        {
            Debug.Log($"=== PLAYER STATS ===\n" +
                     $"Name: {PlayerName}\n" +
                     $"Enemies Killed: {EnemiesKilled}\n" +
                     $"Zones Cleansed: {ZonesCleansed}\n" +
                     $"Time Played: {GetFormattedDuration()}\n" +
                     $"Game Active: {IsGameActive}\n" +
                     $"Has Died: {HasDied}");
        }

        #endregion
    }
}