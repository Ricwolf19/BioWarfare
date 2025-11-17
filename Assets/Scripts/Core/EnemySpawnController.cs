using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Manages enemy spawning within an infected zone
    /// Supports normal enemies and boss enemies with spawn animations
    /// </summary>
    public class EnemySpawnController : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        private InfectedZoneData zoneData;
        private Transform[] spawnPoints;
        private GameObject[] enemyPrefabsToSpawn; // Optional override

        [Header("Spawn State")]
        [SerializeField] private bool isSpawning = false;
        [SerializeField] private bool isPaused = false;
        [SerializeField] private int currentEnemyCount = 0;
        [SerializeField] private int totalSpawned = 0;
        [SerializeField] private bool bossesSpawned = false;

        [Header("Advanced Settings")]
        [SerializeField] private float minDistanceFromPlayer = 5f;
        
        private Transform playerTransform;
        private Coroutine spawnCoroutine;
        private List<GameObject> spawnedBosses = new List<GameObject>();

        #region Initialization

        public void Initialize(InfectedZoneData data, Transform[] points, GameObject[] enemyPrefabs = null)
        {
            zoneData = data;
            spawnPoints = points;
            enemyPrefabsToSpawn = enemyPrefabs; // Store override prefabs

            // Find player reference
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        #endregion

        #region Spawn Control

        /// <summary>
        /// Starts enemy spawning loop
        /// </summary>
        public void StartSpawning()
        {
            if (isSpawning) return;

            isSpawning = true;
            isPaused = false;
            spawnCoroutine = StartCoroutine(SpawnLoop());

            Debug.Log($"[SpawnController] Started spawning for zone");
        }

        /// <summary>
        /// Pauses spawning without clearing enemies
        /// </summary>
        public void PauseSpawning()
        {
            isPaused = true;
            Debug.Log($"[SpawnController] Paused");
        }

        /// <summary>
        /// Resumes spawning
        /// </summary>
        public void ResumeSpawning()
        {
            isPaused = false;
            Debug.Log($"[SpawnController] Resumed");
        }

        /// <summary>
        /// Stops spawning completely
        /// </summary>
        public void StopSpawning()
        {
            isSpawning = false;
            isPaused = false;

            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }

            Debug.Log($"[SpawnController] Stopped. Total spawned: {totalSpawned}");
        }

        #endregion

        #region Spawn Logic

        /// <summary>
        /// Main spawn loop for normal enemies
        /// </summary>
        private IEnumerator SpawnLoop()
        {
            while (isSpawning)
            {
                // Safety check: wait if zoneData not initialized
                if (zoneData == null)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                // Wait for interval
                yield return new WaitForSeconds(zoneData.spawnInterval);

                // Skip if paused or at max capacity
                if (isPaused || currentEnemyCount >= zoneData.maxEnemies)
                    continue;

                // Spawn enemy
                SpawnEnemy();
            }
        }

        /// <summary>
        /// Spawns a single normal enemy at a valid spawn point
        /// </summary>
        private void SpawnEnemy()
        {
            // Use override prefabs if provided, otherwise use zoneData
            GameObject[] prefabsToUse = (enemyPrefabsToSpawn != null && enemyPrefabsToSpawn.Length > 0) 
                ? enemyPrefabsToSpawn 
                : (zoneData != null ? zoneData.enemyPrefabs : null);
            
            if (prefabsToUse == null || prefabsToUse.Length == 0)
            {
                Debug.LogWarning("[SpawnController] No enemy prefabs configured!");
                return;
            }

            // Get valid spawn point
            Transform spawnPoint = GetRandomSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("[SpawnController] No valid spawn points!");
                return;
            }

            // Select random enemy prefab
            GameObject enemyPrefab = prefabsToUse[Random.Range(0, prefabsToUse.Length)];

            // Spawn VFX first (if available)
            if (zoneData.normalEnemySpawnVFX != null)
            {
                Instantiate(zoneData.normalEnemySpawnVFX, spawnPoint.position, spawnPoint.rotation);
            }

            // Spawn enemy
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            if (enemy != null)
            {
                currentEnemyCount++;
                totalSpawned++;
                Debug.Log($"[SpawnController] Spawned {enemy.name} ({currentEnemyCount}/{zoneData.maxEnemies})");

                // Track enemy for cleanup
                EnemyTracker tracker = enemy.AddComponent<EnemyTracker>();
                tracker.controller = this;
            }
        }

        /// <summary>
        /// Gets a random spawn point (with distance check from player)
        /// </summary>
        private Transform GetRandomSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
                return null;

            // Try to find spawn point away from player
            List<Transform> validPoints = new List<Transform>();

            foreach (var point in spawnPoints)
            {
                if (point == null) continue;

                // Check distance from player
                if (playerTransform != null)
                {
                    float distance = Vector3.Distance(point.position, playerTransform.position);
                    if (distance >= minDistanceFromPlayer)
                    {
                        validPoints.Add(point);
                    }
                }
                else
                {
                    validPoints.Add(point);
                }
            }

            // Return random valid point, or fallback to any point
            if (validPoints.Count > 0)
                return validPoints[Random.Range(0, validPoints.Count)];
            
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        #endregion

        #region Boss Spawning

        /// <summary>
        /// Spawns boss enemies after pillar destruction
        /// Called by InfectedZoneController when pillar is destroyed
        /// </summary>
        public void SpawnBosses()
        {
            if (bossesSpawned)
            {
                Debug.LogWarning("[SpawnController] Bosses already spawned!");
                return;
            }

            if (zoneData.bossEnemyPrefabs == null || zoneData.bossEnemyPrefabs.Length == 0)
            {
                Debug.Log("[SpawnController] No boss enemies configured for this zone");
                return;
            }

            StartCoroutine(SpawnBossesCoroutine());
        }

        private IEnumerator SpawnBossesCoroutine()
        {
            Debug.Log($"[SpawnController] Spawning {zoneData.bossEnemyPrefabs.Length} boss(es) in {zoneData.bossSpawnDelay} seconds...");
            
            // Wait for dramatic delay
            yield return new WaitForSeconds(zoneData.bossSpawnDelay);

            // Spawn each boss
            for (int i = 0; i < zoneData.bossEnemyPrefabs.Length; i++)
            {
                GameObject bossPrefab = zoneData.bossEnemyPrefabs[i];
                if (bossPrefab == null)
                {
                    Debug.LogWarning($"[SpawnController] Boss prefab {i} is null, skipping");
                    continue;
                }

                // Select random spawn point
                Transform spawnPoint = GetRandomSpawnPoint();
                if (spawnPoint == null)
                {
                    Debug.LogWarning("[SpawnController] No valid spawn point for boss!");
                    continue;
                }

                // Spawn boss VFX (dramatic entrance)
                if (zoneData.bossEnemySpawnVFX != null)
                {
                    Instantiate(zoneData.bossEnemySpawnVFX, spawnPoint.position, spawnPoint.rotation);
                }

                // Small delay for VFX to play
                yield return new WaitForSeconds(0.5f);

                // Spawn boss
                GameObject boss = Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
                
                if (boss != null)
                {
                    spawnedBosses.Add(boss);
                    
                    // Add to enemy count and attach tracker
                    currentEnemyCount++;
                    totalSpawned++;
                    
                    // Attach tracker for death detection
                    EnemyTracker tracker = boss.GetComponent<EnemyTracker>();
                    if (tracker == null)
                        tracker = boss.AddComponent<EnemyTracker>();
                    tracker.controller = this;
                    
                    Debug.Log($"[SpawnController] Boss spawned: {boss.name} ({i + 1}/{zoneData.bossEnemyPrefabs.Length})");
                }

                // Delay between multiple bosses
                if (i < zoneData.bossEnemyPrefabs.Length - 1)
                {
                    yield return new WaitForSeconds(1f);
                }
            }

            bossesSpawned = true;
            Debug.Log($"[SpawnController] All bosses spawned! Total enemies in zone: {currentEnemyCount}");
        }

        /// <summary>
        /// Check if all bosses are defeated
        /// </summary>
        public bool AreAllBossesDefeated()
        {
            if (!bossesSpawned) return false;
            
            // Remove null references (destroyed bosses)
            spawnedBosses.RemoveAll(boss => boss == null);
            
            return spawnedBosses.Count == 0;
        }

        #endregion

        #region Enemy Tracking

        /// <summary>
        /// Called when an enemy dies
        /// </summary>
        public void OnEnemyDied(GameObject enemy)
        {
            currentEnemyCount--;
            Debug.Log($"[SpawnController] Enemy died. Remaining: {currentEnemyCount}");
        }
        /// <summary>
        /// Check if ALL enemies (normal + bosses) are defeated
        /// </summary>
        public bool AreAllEnemiesDefeated()
        {
            return currentEnemyCount <= 0;
        }

        #endregion

        #region Public API

        public int GetActiveEnemyCount() => currentEnemyCount;
        public int GetTotalSpawned() => totalSpawned;
        public bool IsSpawning() => isSpawning && !isPaused;
        public bool AreBossesSpawned() => bossesSpawned;

        #endregion
    }

    /// <summary>
    /// Simple component to track enemy lifecycle
    /// Add this to spawned enemies to notify controller on death
    /// </summary>
    public class EnemyTracker : MonoBehaviour
    {
        public EnemySpawnController controller;

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.OnEnemyDied(gameObject);
            }
        }
    }
}