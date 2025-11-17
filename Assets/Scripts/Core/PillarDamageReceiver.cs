using UnityEngine;
using cowsins; // FPS Engine namespace

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Handles damage to the zone pillar
    /// Integrates with FPS engine's damage system via IDamageable interface
    /// </summary>
    public class PillarDamageReceiver : MonoBehaviour, IDamageable
    {
        [Header("Pillar Stats")]
        [SerializeField] private float maxHealth = 500f;
        [SerializeField] private float currentHealth;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject destructionVFX;
        [SerializeField] private Material[] damageMaterials; // Progressive damage states
        [SerializeField] private MeshRenderer pillarRenderer;

        [Header("Audio")]
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip destructionSound;
        private AudioSource audioSource;

        private InfectedZoneController parentZone;
        private bool isDestroyed = false;

        #region Initialization

        public void Initialize(InfectedZoneController zone, float health, GameObject overrideDestructionVFX = null)
        {
            parentZone = zone;
            maxHealth = health;
            currentHealth = maxHealth;

            // Use override VFX if provided
            if (overrideDestructionVFX != null)
                destructionVFX = overrideDestructionVFX;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            if (pillarRenderer == null)
                pillarRenderer = GetComponent<MeshRenderer>();

            // Ensure collider exists for hit detection
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                // Add box collider if no collider exists
                BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
                Debug.Log($"[Pillar] Added BoxCollider for hit detection");
            }
            
            Debug.Log($"[Pillar] Initialized - HP: {maxHealth}, Collider: {GetComponent<Collider>() != null}");
        }

        #endregion

        #region Damage System (IDamageable Implementation)

        /// <summary>
        /// IDamageable interface - Main damage method for FPS engine
        /// </summary>
        public void Damage(float damage, bool crit)
        {
            if (isDestroyed) return;

            // Apply critical hit multiplier if needed
            float finalDamage = damage;
            if (crit)
            {
                finalDamage *= 1.5f; // 50% more damage on crits
                Debug.Log($"[Pillar] CRITICAL HIT!");
            }

            currentHealth -= finalDamage;
            currentHealth = Mathf.Max(currentHealth, 0);

            Debug.Log($"[Pillar] Took {finalDamage} damage{(crit ? " (CRIT)" : "")}. HP: {currentHealth}/{maxHealth}");

            // Visual and audio feedback
            OnDamaged();

            // Check for destruction
            if (currentHealth <= 0)
            {
                DestroyPillar();
            }
        }

        /// <summary>
        /// Alternative damage method name (both work)
        /// </summary>
        public void TakeDamage(float damage) => Damage(damage, false);

        /// <summary>
        /// IDamageable interface - Called when hit detected with position info
        /// </summary>
        public void Hit(float damage, Vector3 hitPoint, Vector3 hitDirection)
        {
            Debug.Log($"[Pillar] Hit at {hitPoint} with {damage} damage");
            
            // Optional: Spawn hit effect at hit point
            // if (hitVFX != null)
            //     Instantiate(hitVFX, hitPoint, Quaternion.LookRotation(hitDirection));
            
            Damage(damage, false); // Normal hit, not a crit
        }

        #endregion

        #region Visual Feedback

        private void OnDamaged()
        {
            // Play hit sound
            if (hitSound != null && audioSource != null)
                audioSource.PlayOneShot(hitSound);

            // Update material based on health percentage
            UpdateDamageMaterial();
        }

        private void UpdateDamageMaterial()
        {
            if (pillarRenderer == null || damageMaterials == null || damageMaterials.Length == 0)
                return;

            float healthPercent = currentHealth / maxHealth;
            int materialIndex = Mathf.FloorToInt((1 - healthPercent) * damageMaterials.Length);
            materialIndex = Mathf.Clamp(materialIndex, 0, damageMaterials.Length - 1);

            pillarRenderer.material = damageMaterials[materialIndex];
        }

        #endregion

        #region Destruction

        private void DestroyPillar()
        {
            if (isDestroyed) return;
            isDestroyed = true;

            Debug.Log($"[Pillar] Destroyed!");

            // Play destruction VFX
            if (destructionVFX != null)
            {
                Instantiate(destructionVFX, transform.position, Quaternion.identity);
            }

            // Play destruction sound
            if (destructionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(destructionSound);
            }

            // Notify parent zone
            if (parentZone != null)
            {
                parentZone.OnPillarDestroyed();
            }

            // Destroy pillar object (after VFX delay)
            Destroy(gameObject, 2f);
        }

        #endregion

        #region Public API

        public float GetHealthPercent() => currentHealth / maxHealth;
        public bool IsDestroyed() => isDestroyed;
        
        // IDamageable interface properties (required by FPS engine)
        public bool alive => !isDestroyed;
        public float health => currentHealth;
        public float maxHealth_public => maxHealth;

        #endregion
    }
}