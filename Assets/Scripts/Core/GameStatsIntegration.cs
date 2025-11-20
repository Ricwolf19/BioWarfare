using UnityEngine;
using System.Linq;
using EmeraldAI;
using BioWarfare.Stats;
using BioWarfare.InfectedZones;

namespace BioWarfare.Integration
{
    /// <summary>
    /// Integrates PlayerStatsTracker with existing game systems
    /// Automatically tracks enemy kills and zone completions
    /// </summary>
    public class GameStatsIntegration : MonoBehaviour
    {
        private static GameStatsIntegration instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[GameStatsIntegration] Stats integration initialized.");
        }

        private void Start()
        {
            // Subscribe to enemy death events
            SubscribeToEnemyEvents();

            // Subscribe to zone events
            SubscribeToZoneEvents();
        }

        #region Enemy Kill Tracking

        private void SubscribeToEnemyEvents()
        {
            // Find all enemies in scene and subscribe to their death events
            EmeraldSystem[] enemies = FindObjectsByType<EmeraldSystem>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                var health = enemy.GetComponent<EmeraldHealth>();
                if (health != null)
                {
                    health.OnDeath += OnEnemyKilled;
                }
            }

            Debug.Log($"[GameStatsIntegration] Subscribed to {enemies.Length} enemies.");
        }

        private void OnEnemyKilled()
        {
            PlayerStatsTracker.Instance.AddEnemyKill();
        }

        #endregion

        #region Zone Tracking

        private void SubscribeToZoneEvents()
        {
            // Subscribe to GameProgressManager for zone cleansing events
            var progressManager = FindObjectsByType<GameProgressManager>(FindObjectsSortMode.None).FirstOrDefault();
            
            if (progressManager != null)
            {
                progressManager.OnProgressUpdated.AddListener(OnZoneProgressUpdated);
                Debug.Log("[GameStatsIntegration] Subscribed to GameProgressManager.");
            }
            else
            {
                Debug.LogWarning("[GameStatsIntegration] GameProgressManager not found! Zone stats will not be tracked.");
            }
        }

        private void OnZoneProgressUpdated(int cleansedCount, int totalCount)
        {
            // Update player stats to match game progress
            // This ensures stats stay in sync even if tracking started mid-game
            PlayerStatsTracker.Instance.ZonesCleansed = cleansedCount;
        }

        #endregion

        private void OnDestroy()
        {
            // Unsubscribe from all events to prevent memory leaks
            EmeraldSystem[] enemies = FindObjectsByType<EmeraldSystem>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                var health = enemy.GetComponent<EmeraldHealth>();
                if (health != null)
                {
                    health.OnDeath -= OnEnemyKilled;
                }
            }

            // Unsubscribe from progress manager
            var progressManager = FindObjectsByType<GameProgressManager>(FindObjectsSortMode.None).FirstOrDefault();
            if (progressManager != null)
            {
                progressManager.OnProgressUpdated.RemoveListener(OnZoneProgressUpdated);
            }
        }
    }
}