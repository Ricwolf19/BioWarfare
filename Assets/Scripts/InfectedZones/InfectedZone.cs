using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Infected Zone - Core gameplay objective system
    /// Players must clear zones by destroying pillars or capturing points
    /// Ricardo Tapia - UTCH 2025
    /// </summary>
    public class InfectedZone : MonoBehaviour
    {
        [Header("Zone Configuration")]
        [Tooltip("Unique identifier for this zone")]
        public string zoneID = "Zone_01";
        
        [Tooltip("Display name shown to player")]
        public string zoneName = "Infected Sector Alpha";
        
        [Tooltip("Zone activation type")]
        public ZoneType zoneType = ZoneType.DestroyPillars;
        
        public enum ZoneType
        {
            DestroyPillars,  // Player must destroy all pillars
            CapturePoint,    // Player must hold the capture point
            KillAllEnemies   // Player must eliminate all spawned enemies
        }
        
        [Header("Spawn Configuration")]
        [Tooltip("Enemy prefabs to spawn in this zone")]
        public List<GameObject> enemyPrefabs = new List<GameObject>();
        
        [Tooltip("Spawn points where enemies appear")]
        public Transform[] spawnPoints;
        
        [Tooltip("Max enemies alive at once")]
        [Range(1, 20)]
        public int maxEnemiesAlive = 5;
        
        [Tooltip("Total enemies to spawn during zone")]
        [Range(5, 100)]
        public int totalEnemiesToSpawn = 20;
        
        [Tooltip("Time between enemy spawns")]
        [Range(1f, 30f)]
        public float spawnInterval = 5f;
        
        [Header("Objectives")]
        [Tooltip("Pillars that must be destroyed (if using DestroyPillars mode)")]
        public InfectedPillar[] pillars;
        
        [Tooltip("Capture point object (if using CapturePoint mode)")]
        public GameObject capturePointObject;
        
        [Header("Sequential Pillars")]
        [Tooltip("Enable sequential pillar activation")]
        public bool useSequentialPillars = true;
        
        [Header("VFX References")]
        [Tooltip("Ground effect shown when zone is active")]
        public GameObject zoneGroundVFX;
        
        [Tooltip("VFX played when zone is cleared")]
        public GameObject zoneClearVFX;
        
        [Tooltip("Spawn effects for enemies")]
        public GameObject enemySpawnVFXPrefab;
        
        [Header("Audio")]
        public AudioClip zoneActivatedSound;
        public AudioClip zoneClearedSound;
        public AudioClip pillarDestroyedSound;
        
        [Header("Events")]
        public UnityEvent OnZoneActivated;
        public UnityEvent OnZoneCleared;
        public UnityEvent OnZoneCompleted;
        
        // State tracking
        private bool isActive = false;
        private bool isCleared = false;
        private int enemiesSpawned = 0;
        private int enemiesAlive = 0;
        private int pillarsDestroyed = 0;
        private int currentPillarIndex = 0;
        private List<GameObject> activeEnemies = new List<GameObject>();
        private Coroutine spawnCoroutine;
        private AudioSource audioSource;
        
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            
            // Disable ground VFX initially
            if (zoneGroundVFX) zoneGroundVFX.SetActive(false);
            
            // Register pillars
            if (zoneType == ZoneType.DestroyPillars && pillars != null)
            {
                foreach (var pillar in pillars)
                {
                    if (!useSequentialPillars)
                    {
                        pillar.OnPillarDestroyed += OnPillarDestroyed;
                    }
                }
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            // Only activate if player's main collider enters (not bullets/projectiles)
            if (IsPlayer(other) && !isActive && !isCleared)
            {
                Debug.Log($"<color=green>[InfectedZone]</color> Player detected! Activating zone...");
                ActivateZone();
            }
        }
        
        void OnTriggerStay(Collider other)
        {
            // Backup activation method - sometimes OnTriggerEnter is missed
            if (IsPlayer(other) && !isActive && !isCleared)
            {
                Debug.Log($"<color=yellow>[InfectedZone]</color> Player detected in TriggerStay! Activating zone...");
                ActivateZone();
            }
        }
        
        /// <summary>
        /// Checks if the collider belongs to the actual player (not bullets/projectiles)
        /// </summary>
        private bool IsPlayer(Collider collider)
        {
            // Method 1: Check for Player tag AND PlayerMovement component
            if (collider.CompareTag("Player") && collider.GetComponent<cowsins.PlayerMovement>() != null)
            {
                return true;
            }
            
            // Method 2: Check parent for Player tag (in case child collider triggered)
            Transform parent = collider.transform.parent;
            if (parent != null && parent.CompareTag("Player"))
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Activates the infected zone and starts spawning enemies
        /// </summary>
        public void ActivateZone()
        {
            if (isActive) return;
            
            isActive = true;
            
            // Visual feedback
            if (zoneGroundVFX) zoneGroundVFX.SetActive(true);
            
            // Audio feedback
            if (zoneActivatedSound) audioSource.PlayOneShot(zoneActivatedSound);
            
            // Start spawning enemies
            spawnCoroutine = StartCoroutine(SpawnEnemiesRoutine());
            
            // Activate pillars
            if (useSequentialPillars && pillars != null && pillars.Length > 0)
            {
                ActivateNextPillar();
            }
            else if (pillars != null)
            {
                // Activate all pillars at once (old behavior)
                foreach (var pillar in pillars)
                {
                    pillar.ActivatePillar();
                }
            }
            
            // Fire event
            OnZoneActivated?.Invoke();
            
            Debug.Log($"<color=orange>[InfectedZone]</color> Zone '{zoneName}' activated!");
        }
        
        /// <summary>
        /// Activates next pillar in sequence
        /// </summary>
        private void ActivateNextPillar()
        {
            if (currentPillarIndex >= pillars.Length)
            {
                Debug.Log("[InfectedZone] All pillars activated!");
                return;
            }
            
            var pillar = pillars[currentPillarIndex];
            pillar.ActivatePillar();
            pillar.OnPillarDestroyed += OnSequentialPillarDestroyed;
            
            Debug.Log($"[InfectedZone] Activated pillar {currentPillarIndex + 1}/{pillars.Length}");
        }
        
        /// <summary>
        /// Called when a sequential pillar is destroyed
        /// </summary>
        private void OnSequentialPillarDestroyed(InfectedPillar pillar)
        {
            currentPillarIndex++;
            
            // Audio feedback
            if (pillarDestroyedSound) audioSource.PlayOneShot(pillarDestroyedSound);
            
            // Activate next pillar
            if (currentPillarIndex < pillars.Length)
            {
                ActivateNextPillar();
            }
            else
            {
                // All pillars destroyed
                StopSpawning();
            }
            
            CheckZoneCompletion();
        }
        
        /// <summary>
        /// Spawns enemies at random spawn points
        /// </summary>
        private IEnumerator SpawnEnemiesRoutine()
        {
            while (enemiesSpawned < totalEnemiesToSpawn && isActive)
            {
                // Wait if max enemies alive
                while (enemiesAlive >= maxEnemiesAlive)
                {
                    yield return new WaitForSeconds(1f);
                }
                
                // Spawn enemy
                SpawnEnemy();
                
                // Wait before next spawn
                yield return new WaitForSeconds(spawnInterval);
            }
            
            Debug.Log($"<color=yellow>[InfectedZone]</color> All enemies spawned for '{zoneName}'");
        }
        
        /// <summary>
        /// Spawns a single enemy at a random spawn point
        /// </summary>
        private void SpawnEnemy()
        {
            if (enemyPrefabs.Count == 0 || spawnPoints.Length == 0)
            {
                Debug.LogError("[InfectedZone] No enemy prefabs or spawn points assigned!");
                return;
            }
            
            // Pick random enemy and spawn point
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            // Spawn effect
            if (enemySpawnVFXPrefab != null)
            {
                Instantiate(enemySpawnVFXPrefab, spawnPoint.position, Quaternion.identity);
            }
            
            // Instantiate enemy (with slight delay for VFX)
            StartCoroutine(SpawnEnemyDelayed(enemyPrefab, spawnPoint));
        }
        
        /// <summary>
        /// Spawns enemy after VFX delay
        /// </summary>
        private IEnumerator SpawnEnemyDelayed(GameObject enemyPrefab, Transform spawnPoint)
        {
            yield return new WaitForSeconds(0.5f); // Wait for spawn VFX
            
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Track enemy
            activeEnemies.Add(enemy);
            enemiesSpawned++;
            enemiesAlive++;
            
            // Subscribe to death event
            var emeraldHealth = enemy.GetComponent<EmeraldAI.EmeraldHealth>();
            if (emeraldHealth != null)
            {
                emeraldHealth.OnDeath += () => OnEnemyDied(enemy);
            }
            
            Debug.Log($"[InfectedZone] Spawned enemy {enemiesSpawned}/{totalEnemiesToSpawn}");
        }
        
        /// <summary>
        /// Called when an enemy dies
        /// </summary>
        private void OnEnemyDied(GameObject enemy)
        {
            enemiesAlive--;
            activeEnemies.Remove(enemy);
            
            Debug.Log($"[InfectedZone] Enemy killed. {enemiesAlive} remaining in zone.");
            
            // Check if zone objectives completed
            CheckZoneCompletion();
        }
        
        /// <summary>
        /// Called when a pillar is destroyed (non-sequential mode)
        /// </summary>
        private void OnPillarDestroyed(InfectedPillar pillar)
        {
            pillarsDestroyed++;
            
            // Audio feedback
            if (pillarDestroyedSound) audioSource.PlayOneShot(pillarDestroyedSound);
            
            Debug.Log($"[InfectedZone] Pillar destroyed: {pillarsDestroyed}/{pillars.Length}");
            
            // Check if all pillars destroyed
            if (pillarsDestroyed >= pillars.Length)
            {
                StopSpawning();
            }
            
            CheckZoneCompletion();
        }
        
        /// <summary>
        /// Stops enemy spawning
        /// </summary>
        public void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
            
            OnZoneCleared?.Invoke();
            Debug.Log($"<color=green>[InfectedZone]</color> Spawning stopped for '{zoneName}'");
        }
        
        /// <summary>
        /// Checks if zone objectives are complete
        /// </summary>
        private void CheckZoneCompletion()
        {
            if (isCleared) return;
            
            bool objectiveComplete = false;
            
            switch (zoneType)
            {
                case ZoneType.DestroyPillars:
                    int requiredPillars = useSequentialPillars ? pillars.Length : pillarsDestroyed;
                    bool pillarsComplete = useSequentialPillars ? (currentPillarIndex >= pillars.Length) : (pillarsDestroyed >= pillars.Length);
                    objectiveComplete = pillarsComplete && (enemiesAlive == 0);
                    break;
                    
                case ZoneType.KillAllEnemies:
                    objectiveComplete = (enemiesSpawned >= totalEnemiesToSpawn) && (enemiesAlive == 0);
                    break;
                    
                case ZoneType.CapturePoint:
                    // Handled by PointCapture script
                    break;
            }
            
            if (objectiveComplete)
            {
                CompleteZone();
            }
        }
        
        /// <summary>
        /// Marks the zone as completed
        /// </summary>
        public void CompleteZone()
        {
            if (isCleared) return;
            
            isCleared = true;
            isActive = false;
            
            // Visual feedback
            if (zoneGroundVFX) zoneGroundVFX.SetActive(false);
            if (zoneClearVFX)
            {
                Instantiate(zoneClearVFX, transform.position, Quaternion.identity);
            }
            
            // Audio feedback
            if (zoneClearedSound) audioSource.PlayOneShot(zoneClearedSound);
            
            // Fire event
            OnZoneCompleted?.Invoke();
            
            Debug.Log($"<color=cyan>[InfectedZone]</color> Zone '{zoneName}' COMPLETED! 🎉");
        }
        
        // Public API
        public bool IsActive() => isActive;
        public bool IsCleared() => isCleared;
        public int GetEnemiesAlive() => enemiesAlive;
        public float GetCompletionPercent()
        {
            if (zoneType == ZoneType.DestroyPillars)
            {
                int totalPillars = pillars != null ? pillars.Length : 1;
                int destroyedCount = useSequentialPillars ? currentPillarIndex : pillarsDestroyed;
                float pillarPercent = totalPillars > 0 ? (float)destroyedCount / totalPillars : 0;
                float enemyPercent = enemiesAlive == 0 ? 1f : 0f;
                return (pillarPercent + enemyPercent) / 2f;
            }
            return totalEnemiesToSpawn > 0 ? (float)enemiesSpawned / totalEnemiesToSpawn : 0;
        }
    }
}