using UnityEngine;

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Scriptable Object for zone configuration
    /// Makes zones reusable and editable in inspector
    /// </summary>
    [CreateAssetMenu(fileName = "NewZoneData", menuName = "BioWarfare/Infected Zone Data")]
    public class InfectedZoneData : ScriptableObject
    {
        [Header("Zone Identity")]
        [Tooltip("Unique identifier for this zone")]
        public string zoneID;
        
        [Tooltip("Display name shown to player")]
        public string zoneName;
        
        [Tooltip("Zone order for sequential activation (1 = first, 5 = last/final boss)")]
        [Range(1, 10)]
        public int zoneOrder = 1;
        
        [TextArea(2, 4)]
        public string zoneDescription;

        [Header("Capture Settings")]
        [Tooltip("Time in seconds to fully capture the zone")]
        [Range(5f, 120f)]
        public float captureTime = 30f;

        [Header("Pillar Settings")]
        [Tooltip("Health of the zone's pillar")]
        public float pillarMaxHealth = 500f;

        [Header("Normal Enemy Spawn Settings")]
        [Tooltip("Max simultaneous normal enemies in this zone")]
        [Range(1, 20)]
        public int maxEnemies = 5;
        
        [Tooltip("Delay between enemy spawns (seconds)")]
        [Range(1f, 30f)]
        public float spawnInterval = 5f;
        
        [Tooltip("Normal enemy prefabs to spawn (random selection)")]
        public GameObject[] enemyPrefabs;
        
        [Tooltip("Spawn animation/VFX for normal enemies")]
        public GameObject normalEnemySpawnVFX;
        
        [Tooltip("Stop spawning when zone is being captured?")]
        public bool pauseSpawnDuringCapture = true;

        [Header("Boss Enemy Settings")]
        [Tooltip("Boss/Mini-boss enemies to spawn when pillar is destroyed")]
        public GameObject[] bossEnemyPrefabs;
        
        [Tooltip("Spawn animation/VFX for boss enemies")]
        public GameObject bossEnemySpawnVFX;
        
        [Tooltip("Delay before spawning bosses after pillar destruction (seconds)")]
        [Range(0f, 10f)]
        public float bossSpawnDelay = 2f;

        [Header("Visual Effects")]
        public GameObject zoneEnterVFX;
        public GameObject zoneCaptureVFX;
        public GameObject pillarVulnerableVFX; // VFX when pillar becomes vulnerable
        public GameObject zoneCleansedVFX;
        
        [Header("Audio")]
        public AudioClip zoneEnterSound;
        public AudioClip capturingSound;
        public AudioClip zoneCleansedSound;
    }
}