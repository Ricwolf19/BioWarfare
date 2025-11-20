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
            // Subscribe to enemy death events for existing enemies
            SubscribeToEnemyEvents();

            // Subscribe to spawn controllers for dynamically spawned enemies
            SubscribeToSpawnControllers();

            // Subscribe to zone events
            SubscribeToZoneEvents();
        }

        #region Enemy Kill Tracking

        /// <summary>
        /// Subscribe to pre-existing enemies in the scene
        /// </summary>
        private void SubscribeToEnemyEvents()
        {
            // Find all enemies in scene and subscribe to their death events
            EmeraldSystem[] enemies = FindObjectsByType<EmeraldSystem>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                SubscribeToEnemy(enemy.gameObject);
            }

            Debug.Log($"[GameStatsIntegration] Subscribed to {enemies.Length} pre-existing enemies.");
        }

        /// <summary>
        /// Subscribe to all enemy spawn controllers to track dynamically spawned enemies
        /// </summary>
        private void SubscribeToSpawnControllers()
        {
            var spawnControllers = FindObjectsByType<EnemySpawnController>(FindObjectsSortMode.None);
            foreach (var controller in spawnControllers)
            {
                controller.OnEnemySpawned.AddListener(OnEnemySpawnedFromController);
            }

            Debug.Log($"[GameStatsIntegration] Subscribed to {spawnControllers.Length} spawn controllers.");
        }

        /// <summary>
        /// Called when an enemy is spawned by a spawn controller
        /// </summary>
        private void OnEnemySpawnedFromController(GameObject enemy)
        {
            if (enemy != null)
            {
                SubscribeToEnemy(enemy);
                Debug.Log($"[GameStatsIntegration] Subscribed to dynamically spawned enemy: {enemy.name}");
            }
        }

        /// <summary>
        /// Subscribe to a single enemy's death event
        /// </summary>
        private void SubscribeToEnemy(GameObject enemyObject)
        {
            var health = enemyObject.GetComponent<EmeraldHealth>();
            if (health != null)
            {
                health.OnDeath += OnEnemyKilled;
            }
            else
            {
                Debug.LogWarning($"[GameStatsIntegration] Enemy {enemyObject.name} has no EmeraldHealth component!");
            }
        }

        private void OnEnemyKilled()
        {
            PlayerStatsTracker.Instance.AddEnemyKill();
            Debug.Log($"[GameStatsIntegration] Enemy killed. Total: {PlayerStatsTracker.Instance.EnemiesKilled}");
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
            // Unsubscribe from all enemies
            EmeraldSystem[] enemies = FindObjectsByType<EmeraldSystem>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                var health = enemy.GetComponent<EmeraldHealth>();
                if (health != null)
                {
                    health.OnDeath -= OnEnemyKilled;
                }
            }

            // Unsubscribe from spawn controllers
            var spawnControllers = FindObjectsByType<EnemySpawnController>(FindObjectsSortMode.None);
            foreach (var controller in spawnControllers)
            {
                controller.OnEnemySpawned.RemoveListener(OnEnemySpawnedFromController);
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