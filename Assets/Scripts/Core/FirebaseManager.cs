using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;

namespace BioWarfare.Backend
{
    /// <summary>
    /// Manages Firebase Firestore integration
    /// Handles player stats upload to leaderboard
    /// </summary>
    public class FirebaseManager : MonoBehaviour
    {
        private static FirebaseManager instance;
        public static FirebaseManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("FirebaseManager");
                    instance = go.AddComponent<FirebaseManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Firebase Status")]
        [SerializeField] private bool isInitialized = false;
        [SerializeField] private bool useFirebase = true; // Toggle for testing without Firebase

        private FirebaseFirestore db;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (useFirebase)
            {
                InitializeFirebase();
            }
        }

        #region Initialization

        private void InitializeFirebase()
        {
            Debug.Log("[FirebaseManager] Initializing Firebase...");

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    db = FirebaseFirestore.DefaultInstance;
                    isInitialized = true;
                    Debug.Log("[FirebaseManager] Firebase initialized successfully!");
                }
                else
                {
                    Debug.LogError($"[FirebaseManager] Could not resolve Firebase dependencies: {dependencyStatus}");
                    isInitialized = false;
                }
            });
                    
            Debug.LogWarning("[FirebaseManager] Firebase SDK not installed. Install via Package Manager.");
            isInitialized = false;
        }

        #endregion

        #region Upload Stats

        /// <summary>
        /// Upload player stats to Firestore
        /// </summary>
        public async Task<bool> UploadPlayerStats(string playerName, int enemiesKilled, int zonesCleansed, float timePlayed, bool died)
        {
            if (!useFirebase)
            {
                Debug.Log("[FirebaseManager] Firebase disabled, skipping upload.");
                return false;
            }

            if (!isInitialized)
            {
                Debug.LogError("[FirebaseManager] Firebase not initialized!");
                return false;
            }

            Debug.Log($"[FirebaseManager] Uploading stats for {playerName}...");

            try
            {
                // Create player stats document
                Dictionary<string, object> statsData = new Dictionary<string, object>
                {
                    { "playerName", playerName },
                    { "enemiesKilled", enemiesKilled },
                    { "zonesCleansed", zonesCleansed },
                    { "timePlayed", timePlayed },
                    { "died", died },
                    { "timestamp", FieldValue.ServerTimestamp },
                    { "platform", Application.platform.ToString() }
                };

                // Upload to Firestore collection "leaderboard"
                DocumentReference docRef = await db.Collection("leaderboard").AddAsync(statsData);

                Debug.Log($"[FirebaseManager] Stats uploaded successfully! Document ID: {docRef.Id}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FirebaseManager] Failed to upload stats: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Quick upload using PlayerStatsTracker data
        /// </summary>
        public async Task<bool> UploadCurrentGameStats()
        {
            var stats = Stats.PlayerStatsTracker.Instance;

            if (string.IsNullOrEmpty(stats.PlayerName))
            {
                Debug.LogWarning("[FirebaseManager] Cannot upload stats: Player name not set! Did you start from MainMenu?");
                Debug.LogWarning("[FirebaseManager] To fix: Always start game from MainMenu scene, or PlayerStatsTracker will auto-generate a name.");
                return false;
            }

            return await UploadPlayerStats(
                stats.PlayerName,
                stats.EnemiesKilled,
                stats.ZonesCleansed,
                stats.GetGameDuration(),
                stats.HasDied
            );
        }

        #endregion

        #region Leaderboard (Optional)

        /// <summary>
        /// Get top 10 players from leaderboard
        /// </summary>
        public async Task<List<LeaderboardEntry>> GetLeaderboard()
        {
            if (!isInitialized)
            {
                Debug.LogError("[FirebaseManager] Firebase not initialized!");
                return new List<LeaderboardEntry>();
            }
            
            try
            {
                Query query = db.Collection("leaderboard")
                    .OrderByDescending("enemiesKilled")
                    .Limit(10);

                QuerySnapshot snapshot = await query.GetSnapshotAsync();
                
                List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    Dictionary<string, object> data = document.ToDictionary();
                    leaderboard.Add(new LeaderboardEntry
                    {
                        PlayerName = data["playerName"].ToString(),
                        EnemiesKilled = System.Convert.ToInt32(data["enemiesKilled"]),
                        ZonesCleansed = System.Convert.ToInt32(data["zonesCleansed"]),
                        TimePlayed = System.Convert.ToSingle(data["timePlayed"])
                    });
                }

                Debug.Log($"[FirebaseManager] Retrieved {leaderboard.Count} leaderboard entries.");
                return leaderboard;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FirebaseManager] Failed to get leaderboard: {e.Message}");
                return new List<LeaderboardEntry>();
            }
            
        }

        #endregion
    }

    /// <summary>
    /// Leaderboard entry data structure
    /// </summary>
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string PlayerName;
        public int EnemiesKilled;
        public int ZonesCleansed;
        public float TimePlayed;
    }
}