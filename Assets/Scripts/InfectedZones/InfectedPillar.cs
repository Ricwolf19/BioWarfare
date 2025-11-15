using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Destroyable Pillar - Standalone Health + Capture Mechanic
    /// Player must capture to make vulnerable, then destroy with bullets
    /// Ricardo Tapia - UTCH 2025
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InfectedPillar : MonoBehaviour
    {
        [Header("Pillar State")]
        public bool isActive = false;
        private bool isVulnerable = false;
        
        [Header("Health Configuration")]
        [Tooltip("Max health of pillar")]
        [Range(100f, 2000f)]
        public float maxHealth = 500f;
        
        private float currentHealth;
        
        [Header("Capture Configuration")]
        [Tooltip("Time to hold position to capture")]
        [Range(5f, 60f)]
        public float captureTime = 15f;
        
        [Tooltip("Capture radius")]
        [Range(3f, 20f)]
        public float captureRadius = 8f;
        
        [Tooltip("Health regen per second when player leaves")]
        [Range(0f, 50f)]
        public float healthRegenRate = 10f;
        
        [Header("Health Bar UI (Optional)")]
        [Tooltip("Canvas with health bar slider")]
        public Canvas healthBarCanvas;
        
        [Tooltip("Slider for health bar")]
        public Slider healthBarSlider;
        
        [Header("VFX Prefabs - DRAG FROM PROJECT")]
        public GameObject activeVFXPrefab;        // StormPillar
        public GameObject captureVFXPrefab;       // HealingSeal
        public GameObject vulnerableVFXPrefab;    // FusionCore
        public GameObject destroyVFXPrefab;       // Explosion
        
        [Header("Visual Settings")]
        [Tooltip("Hide the cylinder mesh and only show VFX")]
        public bool hidePillarMesh = true;
        
        [Header("Checkpoint Indicator")]
        public GameObject checkpointPrefab;       // CheckpointView prefab
        
        [Header("Audio")]
        public AudioClip captureSound;
        public AudioClip vulnerableSound;
        public AudioClip destroySound;
        
        [Header("Events")]
        public UnityEvent OnCaptured;
        public UnityEvent OnDestroyed;
        
        // Events for zone system
        public event Action<InfectedPillar> OnPillarCaptured;
        public event Action<InfectedPillar> OnPillarDestroyed;
        
        // State
        private bool isDestroyed = false;
        private bool isCapturing = false;
        private float captureProgress = 0f;
        private Transform playerTransform;
        private AudioSource audioSource;
        private Renderer pillarRenderer;
        private Color originalColor;
        
        // VFX instances (spawned at runtime)
        private GameObject activeVFXInstance;
        private GameObject captureVFXInstance;
        private GameObject checkpointInstance;
        
        void Start()
        {
            // Initialize health
            currentHealth = maxHealth;
            
            // Get components
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            
            pillarRenderer = GetComponentInChildren<Renderer>();
            if (pillarRenderer != null)
            {
                originalColor = pillarRenderer.material.color;
                
                // Hide mesh if option is enabled (only show VFX)
                if (hidePillarMesh)
                {
                    pillarRenderer.enabled = false;
                    Debug.Log("[InfectedPillar] Pillar mesh hidden. VFX-only mode.");
                }
            }
            
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            
            // Hide health bar initially
            if (healthBarCanvas != null) healthBarCanvas.enabled = false;
            
            // Initially invulnerable
            SetVulnerable(false);
            
            // Don't activate yet (ZoneManager will activate)
            if (!isActive)
            {
                gameObject.SetActive(false);
            }
        }
        
        void Update()
        {
            if (!isActive || isDestroyed || isVulnerable) return;
            
            // Check if player is in capture radius
            if (playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                
                if (distance <= captureRadius)
                {
                    // Start capturing
                    if (!isCapturing)
                    {
                        StartCapture();
                    }
                    
                    // Increase capture progress
                    captureProgress += Time.deltaTime;
                    
                    // Check if capture complete
                    if (captureProgress >= captureTime)
                    {
                        CompleteCapture();
                    }
                }
                else
                {
                    // Player left capture zone
                    if (isCapturing)
                    {
                        StopCapture();
                    }
                }
            }
            
            // Health regeneration when vulnerable but player not capturing
            if (isVulnerable && !isCapturing && healthRegenRate > 0 && currentHealth < maxHealth)
            {
                currentHealth += healthRegenRate * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
                UpdateHealthBar();
                UpdateVisuals();
            }
        }
        
        /// <summary>
        /// Activates this pillar and spawns checkpoint indicator
        /// </summary>
        public void ActivatePillar()
        {
            isActive = true;
            gameObject.SetActive(true);
            
            // Spawn active VFX
            if (activeVFXPrefab != null)
            {
                activeVFXInstance = Instantiate(activeVFXPrefab, transform.position, Quaternion.identity, transform);
                
                // Disable looping on VFX - let it play once then stay
                DisableVFXLoop(activeVFXInstance);
                
                Debug.Log("[InfectedPillar] Active VFX spawned");
            }
            
            // Spawn checkpoint indicator
            SpawnCheckpoint();
            
            Debug.Log($"<color=yellow>[InfectedPillar]</color> Pillar activated at {transform.position}");
        }
        
        /// <summary>
        /// Spawns CheckpointView indicator above pillar
        /// </summary>
        private void SpawnCheckpoint()
        {
            if (checkpointPrefab == null)
            {
                Debug.LogWarning("[InfectedPillar] No checkpoint prefab assigned!");
                return;
            }
            
            // Spawn above pillar
            Vector3 spawnPos = transform.position + Vector3.up * 10f;
            checkpointInstance = Instantiate(checkpointPrefab, spawnPos, Quaternion.identity);
            
            Debug.Log("[InfectedPillar] Checkpoint indicator spawned");
        }
        
        /// <summary>
        /// Starts the capture process
        /// </summary>
        private void StartCapture()
        {
            isCapturing = true;
            captureProgress = 0f;
            
            // Spawn capture VFX (HealingSeal)
            if (captureVFXPrefab != null)
            {
                captureVFXInstance = Instantiate(captureVFXPrefab, transform.position, Quaternion.identity, transform);
            }
            
            // Audio feedback
            if (captureSound) audioSource.PlayOneShot(captureSound);
            
            Debug.Log("[InfectedPillar] Capture started!");
        }
        
        /// <summary>
        /// Stops the capture process
        /// </summary>
        private void StopCapture()
        {
            isCapturing = false;
            captureProgress = 0f;
            
            // Destroy capture VFX
            if (captureVFXInstance != null)
            {
                Destroy(captureVFXInstance);
                captureVFXInstance = null;
            }
            
            Debug.Log("[InfectedPillar] Capture stopped (player left)");
        }
        
        /// <summary>
        /// Completes capture - pillar becomes vulnerable
        /// </summary>
        private void CompleteCapture()
        {
            isCapturing = false;
            SetVulnerable(true);
            
            // Destroy capture VFX
            if (captureVFXInstance != null)
            {
                Destroy(captureVFXInstance);
                captureVFXInstance = null;
            }
            
            // Spawn vulnerable VFX
            if (vulnerableVFXPrefab != null)
            {
                if (activeVFXInstance != null)
                {
                    Destroy(activeVFXInstance); // Remove active VFX
                }
                activeVFXInstance = Instantiate(vulnerableVFXPrefab, transform.position, Quaternion.identity, transform);
                
                // Disable looping on vulnerable VFX
                DisableVFXLoop(activeVFXInstance);
                
                Debug.Log("[InfectedPillar] Vulnerable VFX spawned");
            }
            
            // Audio feedback
            if (vulnerableSound) audioSource.PlayOneShot(vulnerableSound);
            
            // Fire events
            OnCaptured?.Invoke();
            OnPillarCaptured?.Invoke(this);
            
            Debug.Log("<color=green>[InfectedPillar]</color> Capture COMPLETE! Pillar now vulnerable to damage!");
        }
        
        /// <summary>
        /// Sets pillar vulnerability (can take damage or not)
        /// </summary>
        private void SetVulnerable(bool vulnerable)
        {
            isVulnerable = vulnerable;
            
            // Show/hide health bar
            if (healthBarCanvas != null)
            {
                healthBarCanvas.enabled = vulnerable;
            }
            
            // Reset health if becoming invulnerable
            if (!vulnerable)
            {
                currentHealth = maxHealth;
                UpdateHealthBar();
            }
            
            UpdateVisuals();
        }
        
        /// <summary>
        /// Damages the pillar (called by FPS Engine bullets)
        /// </summary>
        public void Damage(float damageAmount)
        {
            if (!isVulnerable || isDestroyed) return;
            
            currentHealth -= damageAmount;
            currentHealth = Mathf.Max(currentHealth, 0);
            
            UpdateHealthBar();
            UpdateVisuals();
            
            Debug.Log($"[InfectedPillar] Took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");
            
            // Check if destroyed
            if (currentHealth <= 0)
            {
                DestroyPillar();
            }
        }
        
        /// <summary>
        /// Updates health bar UI
        /// </summary>
        private void UpdateHealthBar()
        {
            if (healthBarSlider != null)
            {
                healthBarSlider.value = currentHealth / maxHealth;
            }
        }
        
        /// <summary>
        /// Updates visual feedback based on health
        /// </summary>
        private void UpdateVisuals()
        {
            if (pillarRenderer == null) return;
            
            float healthPercent = currentHealth / maxHealth;
            
            if (isVulnerable)
            {
                // Lerp color from red (low health) to yellow (high health)
                Color damageColor = Color.Lerp(Color.red, Color.yellow, healthPercent);
                pillarRenderer.material.color = damageColor;
            }
            else
            {
                // Invulnerable - show original color
                pillarRenderer.material.color = originalColor;
            }
        }
        
        /// <summary>
        /// Destroys the pillar
        /// </summary>
        private void DestroyPillar()
        {
            if (isDestroyed) return;
            
            isDestroyed = true;
            
            // Destroy checkpoint indicator
            if (checkpointInstance != null)
            {
                Destroy(checkpointInstance);
            }
            
            // Destroy all VFX
            if (activeVFXInstance != null) Destroy(activeVFXInstance);
            if (captureVFXInstance != null) Destroy(captureVFXInstance);
            
            // Spawn destroy VFX
            if (destroyVFXPrefab != null)
            {
                Instantiate(destroyVFXPrefab, transform.position, Quaternion.identity);
            }
            
            // Audio feedback
            if (destroySound) audioSource.PlayOneShot(destroySound);
            
            // Fire events
            OnDestroyed?.Invoke();
            OnPillarDestroyed?.Invoke(this);
            
            Debug.Log("<color=cyan>[InfectedPillar]</color> Pillar DESTROYED! 💥");
            
            // Destroy GameObject after delay
            Destroy(gameObject, 2f);
        }
        
        /// <summary>
        /// Makes VFX play appear animation then stay visible in loop state
        /// </summary>
        private void DisableVFXLoop(GameObject vfxInstance)
        {
            if (vfxInstance == null) return;
            
            // Get all Animators in VFX
            Animator[] animators = vfxInstance.GetComponentsInChildren<Animator>();
            foreach (var anim in animators)
            {
                if (anim == null) continue;
                
                // Check if animator has parameters
                if (anim.parameters.Length > 0)
                {
                    // Try to set trigger/bool for staying active
                    foreach (var param in anim.parameters)
                    {
                        if (param.name.ToLower().Contains("loop") && param.type == AnimatorControllerParameterType.Bool)
                        {
                            anim.SetBool(param.name, true);
                            Debug.Log($"[InfectedPillar] Set animator parameter '{param.name}' to true");
                        }
                        else if (param.name.ToLower().Contains("hide") && param.type == AnimatorControllerParameterType.Bool)
                        {
                            anim.SetBool(param.name, false);
                            Debug.Log($"[InfectedPillar] Set animator parameter '{param.name}' to false");
                        }
                    }
                }
                
                // Alternative: Force animator to stay in specific state
                // Play "Appear" animation, then it should transition to "Loop"
                if (HasState(anim, "Loop"))
                {
                    // Give appear animation time to play (0.5 seconds), then force loop
                    StartCoroutine(ForceLoopState(anim));
                }
            }
        }
        
        /// <summary>
        /// Checks if animator has a specific state
        /// </summary>
        private bool HasState(Animator anim, string stateName)
        {
            if (anim == null || anim.runtimeAnimatorController == null) return false;
            
            foreach (var clip in anim.runtimeAnimatorController.animationClips)
            {
                if (clip.name.ToLower().Contains(stateName.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Forces animator to stay in Loop state after appear animation
        /// </summary>
        private IEnumerator ForceLoopState(Animator anim)
        {
            // Wait for appear animation to finish (most appear animations are ~1 second)
            yield return new WaitForSeconds(1.5f);
            
            // Force play Loop state
            if (anim != null)
            {
                anim.Play("Loop", 0);
                Debug.Log("[InfectedPillar] Forced animator to Loop state");
            }
        }
        
        // Public API
        public bool IsVulnerable() => isVulnerable;
        public bool IsCapturing() => isCapturing;
        public float GetCaptureProgress() => captureProgress / captureTime;
        public float GetHealthPercent() => currentHealth / maxHealth;
        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        
        // Debug visualization
        void OnDrawGizmosSelected()
        {
            if (!isActive) return;
            
            // Draw capture radius
            Gizmos.color = isVulnerable ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, captureRadius);
        }
    }
}