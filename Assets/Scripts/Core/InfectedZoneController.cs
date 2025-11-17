using UnityEngine;
using System.Collections;

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Main controller for infected zone - STANDALONE (no PointCapture inheritance)
    /// Clean, modular design following DRY principles
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(ZoneCaptureSystem))]
    public class InfectedZoneController : MonoBehaviour
    {
        [Header("Zone Configuration")]
        [SerializeField] private InfectedZoneData zoneData;
        
        [Header("Zone Components")]
        [SerializeField] private Transform[] enemySpawnPoints;
        [SerializeField] private Transform checkpointMarker;
        [SerializeField] private float checkpointHeightOffset = 5f; // Height above zone center
        
        [Header("Pillar Reference")]
        [SerializeField] private GameObject pillarObject; // Reference to pillar already in scene
        
        [Header("Zone State")]
        [SerializeField] private ZoneState currentState = ZoneState.Locked;
        [SerializeField] private ZoneEvents zoneEvents;

        // Component references
        private ZoneCaptureSystem captureSystem;
        private EnemySpawnController spawnController;
        private PillarDamageReceiver pillarDamageReceiver;
        private AudioSource audioSource;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            RegisterWithManager();
            ConfigureFromZoneData();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            // Get/add capture system
            captureSystem = GetComponent<ZoneCaptureSystem>();
            if (captureSystem == null)
                captureSystem = gameObject.AddComponent<ZoneCaptureSystem>();

            // Get/add spawn controller
            spawnController = GetComponent<EnemySpawnController>();
            if (spawnController == null)
                spawnController = gameObject.AddComponent<EnemySpawnController>();

            // Get/add audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            // Setup collider
            SphereCollider trigger = GetComponent<SphereCollider>();
            trigger.isTrigger = true;

            // Subscribe to capture events
            captureSystem.OnCaptureStarted.AddListener(OnCaptureStarted);
            captureSystem.OnCaptureCompleted.AddListener(OnCaptureCompleted);
            
            // Setup pillar reference if assigned
            if (pillarObject != null)
            {
                pillarDamageReceiver = pillarObject.GetComponent<PillarDamageReceiver>();
                if (pillarDamageReceiver == null)
                    pillarDamageReceiver = pillarObject.AddComponent<PillarDamageReceiver>();
                
                // Initialize pillar (visible from start, VFX set on pillar GameObject in inspector)
                pillarDamageReceiver.Initialize(this, zoneData.pillarMaxHealth);
                Debug.Log($"[Zone {zoneData?.zoneName}] Pillar initialized and visible");
            }
        }

        private void RegisterWithManager()
        {
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.RegisterZone(this);
            }
            else
            {
                Debug.LogError("[InfectedZone] GameProgressManager not found!");
            }
        }

        private void ConfigureFromZoneData()
        {
            if (zoneData == null) return;

            // Configure capture system
            captureSystem.SetCaptureTime(zoneData.captureTime);

            // Configure spawn controller (but don't start spawning yet!)
            spawnController.Initialize(zoneData, enemySpawnPoints, null);
        }

        #endregion

        #region State Management

        public void SetState(ZoneState newState)
        {
            if (currentState == newState) return;

            ZoneState previousState = currentState;
            currentState = newState;

            Debug.Log($"[Zone {zoneData?.zoneName}] State: {previousState} → {newState}");

            switch (newState)
            {
                case ZoneState.Active:
                    ActivateZone();
                    break;
                case ZoneState.Capturing:
                    StartCapturingZone();
                    break;
                case ZoneState.PillarVulnerable:
                    MakePillarVulnerable();
                    break;
                case ZoneState.Cleansed:
                    CleanseZone();
                    break;
            }
        }

        #endregion

        #region Zone State Handlers

        private void ActivateZone()
        {
            Debug.Log($"[Zone {zoneData?.zoneName}] Activating zone (Ready for player to enter)...");

            // Show checkpoint
            ShowCheckpointMarker(true);

            // DON'T start spawning here - wait for player to enter!
            // Spawning will start in OnTriggerEnter when player enters

            // Play VFX
            if (zoneData?.zoneEnterVFX != null)
                Instantiate(zoneData.zoneEnterVFX, transform.position, Quaternion.identity);

            zoneEvents.OnZoneActivated?.Invoke();
        }

        private void StartCapturingZone()
        {
            // Pause spawns during capture
            if (zoneData.pauseSpawnDuringCapture)
                spawnController.PauseSpawning();

            // Play capture audio
            if (zoneData?.capturingSound != null && audioSource != null)
            {
                audioSource.clip = zoneData.capturingSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            zoneEvents.OnCaptureStarted?.Invoke();
        }

        private void MakePillarVulnerable()
        {
            // Stop audio
            if (audioSource != null)
                audioSource.Stop();

            // Show capture complete VFX
            if (zoneData?.zoneCaptureVFX != null)
            {
                Instantiate(zoneData.zoneCaptureVFX, transform.position, Quaternion.identity);
                Debug.Log($"[Zone {zoneData?.zoneName}] Capture VFX spawned at {transform.position}");
            }
            else
            {
                Debug.LogWarning($"[Zone {zoneData?.zoneName}] Zone Capture VFX is NULL!");
            }

            // Pillar is now vulnerable to damage after capture
            if (pillarObject != null)
            {
                Debug.Log($"[Zone {zoneData?.zoneName}] Pillar is now vulnerable to damage!");
                
                // Show pillar vulnerable VFX at pillar position
                if (zoneData?.pillarVulnerableVFX != null)
                {
                    Instantiate(zoneData.pillarVulnerableVFX, pillarObject.transform.position, Quaternion.identity);
                    Debug.Log($"[Zone {zoneData?.zoneName}] Pillar Vulnerable VFX spawned at pillar position");
                }
            }
            else
            {
                Debug.LogWarning($"[Zone {zoneData?.zoneName}] Pillar object is NULL!");
            }

            // Resume spawning
            spawnController.ResumeSpawning();

            zoneEvents.OnCaptureCompleted?.Invoke();
        }

        private void CleanseZone()
        {
            Debug.Log($"[Zone {zoneData?.zoneName}] ✨ CLEANSING ZONE...");
            
            // Stop spawning
            spawnController.StopSpawning();

            // DESTROY checkpoint marker (not just hide it)
            DestroyCheckpointMarker();

            // Show zone cleansed VFX
            if (zoneData?.zoneCleansedVFX != null)
            {
                Instantiate(zoneData.zoneCleansedVFX, transform.position, Quaternion.identity);
                Debug.Log($"[Zone {zoneData?.zoneName}] Cleansed VFX spawned at {transform.position}");
            }
            else
            {
                Debug.LogWarning($"[Zone {zoneData?.zoneName}] Zone Cleansed VFX is NULL!");
            }

            // Audio
            if (zoneData?.zoneCleansedSound != null && audioSource != null)
            {
                audioSource.loop = false;
                audioSource.PlayOneShot(zoneData.zoneCleansedSound);
                Debug.Log($"[Zone {zoneData?.zoneName}] Cleansed sound played");
            }

            // Notify manager to update progress tracking
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.OnZoneCleansed(this);
                Debug.Log($"[Zone {zoneData?.zoneName}] Notified GameProgressManager - Zone Complete!");
            }

            zoneEvents.OnZoneCleansed?.Invoke();

            // Disable trigger
            GetComponent<Collider>().enabled = false;
        }

        #endregion

        #region Checkpoint Marker

        public void ShowCheckpointMarker(bool show)
        {
            if (checkpointMarker == null)
            {
                Debug.LogWarning($"[Zone {zoneData?.zoneName}] Checkpoint marker is NULL! Assign it in the Inspector.");
                return;
            }
            
            checkpointMarker.gameObject.SetActive(show);
            
            // Position marker above the zone center
            if (show)
            {
                Vector3 markerPos = transform.position + Vector3.up * checkpointHeightOffset;
                checkpointMarker.position = markerPos;
                Debug.Log($"[Zone {zoneData?.zoneName}] Checkpoint marker SHOWN at {markerPos} (height offset: {checkpointHeightOffset})");
            }
            else
            {
                Debug.Log($"[Zone {zoneData?.zoneName}] Checkpoint marker HIDDEN");
            }
        }

        private void DestroyCheckpointMarker()
        {
            if (checkpointMarker != null)
            {
                Debug.Log($"[Zone {zoneData?.zoneName}] Checkpoint marker DESTROYED");
                Destroy(checkpointMarker.gameObject);
                checkpointMarker = null;
            }
        }

        #endregion

        #region Trigger Detection

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            // Only process if zone is Active or already Capturing (re-entry)
            if (currentState == ZoneState.Active || currentState == ZoneState.Capturing)
            {
                Debug.Log($"[Zone] Player entered: {zoneData?.zoneName} (State: {currentState})");
                
                captureSystem.StartCapture();
                
                // Only transition to Capturing state if we're currently Active
                if (currentState == ZoneState.Active)
                {
                    // START SPAWNING when player first enters the zone
                    Debug.Log($"[Zone] First entry - starting enemy spawning");
                    spawnController.StartSpawning();
                    
                    SetState(ZoneState.Capturing);

                    // Play enter sound only on first entry
                    if (zoneData?.zoneEnterSound != null && audioSource != null)
                        audioSource.PlayOneShot(zoneData.zoneEnterSound);

                    zoneEvents.OnPlayerEntered?.Invoke();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            // Only process exit if we're in a capturing state
            if (currentState == ZoneState.Active || currentState == ZoneState.Capturing)
            {
                Debug.Log($"[Zone] Player exited (Progress will be preserved)");
                
                captureSystem.StopCapture();
                zoneEvents.OnPlayerExited?.Invoke();
            }
        }

        #endregion

        #region Capture Event Handlers

        private void OnCaptureStarted()
        {
            Debug.Log($"[Zone] Capture started");
        }

        private void OnCaptureCompleted()
        {
            Debug.Log($"[Zone] Capture completed - pillar is now vulnerable");
            SetState(ZoneState.PillarVulnerable);
            
            // NOTE: Zone is NOT cleansed yet - must destroy pillar and defeat bosses first!
        }

        #endregion

        #region Public API

        public void ActivateFromManager()
        {
            if (currentState == ZoneState.Locked)
                SetState(ZoneState.Active);
        }

        public void OnPillarDestroyed()
        {
            Debug.Log($"[Zone {zoneData?.zoneName}] 💥 PILLAR DESTROYED! Zone complete!");
            
            zoneEvents.OnPillarDestroyed?.Invoke();
            
            // Stop normal enemy spawning
            spawnController.StopSpawning();
            
            // Spawn bosses (optional challenge content)
            spawnController.SpawnBosses();
            
            Debug.Log($"[Zone {zoneData?.zoneName}] Bosses spawned as optional challenge");
            
            // ✅ ZONE IS NOW COMPLETE - Mark it immediately!
            SetState(ZoneState.Cleansed);
        }

        public ZoneState GetState() => currentState;
        public InfectedZoneData GetZoneData() => zoneData;
        public bool IsCleansed() => currentState == ZoneState.Cleansed;
        public float GetCaptureProgress() => captureSystem.GetProgress();

        #endregion


        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = currentState switch
            {
                ZoneState.Locked => Color.gray,
                ZoneState.Active => Color.yellow,
                ZoneState.Capturing => Color.blue,
                ZoneState.PillarVulnerable => Color.red,
                ZoneState.Cleansed => Color.green,
                _ => Color.white
            };

            Gizmos.DrawWireSphere(transform.position, GetComponent<SphereCollider>()?.radius ?? 10f);
        }

        #endregion
    }
}