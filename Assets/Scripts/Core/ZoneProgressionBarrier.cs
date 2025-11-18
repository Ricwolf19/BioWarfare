using UnityEngine;
using System.Collections.Generic;

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Invisible wall that blocks progression until specific zones are cleansed
    /// Automatically destroys itself when requirements are met
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ZoneProgressionBarrier : MonoBehaviour
    {
        [Header("Barrier Requirements")]
        [Tooltip("Zones that must be cleansed before this barrier is removed")]
        [SerializeField] private InfectedZoneController[] requiredZones;

        [Tooltip("Destroy barrier immediately when all zones are cleansed?")]
        [SerializeField] private bool autoDestroy = true;

        [Tooltip("Play VFX when barrier is destroyed?")]
        [SerializeField] private GameObject destroyVFX;

        [Tooltip("Play audio when barrier is destroyed?")]
        [SerializeField] private AudioClip destroySound;

        [Header("Visual Feedback")]
        [Tooltip("Optional: Material for the barrier (use transparent/force field shader)")]
        [SerializeField] private Material barrierMaterial;

        [Tooltip("Show visual barrier (or keep invisible)?")]
        [SerializeField] private bool showVisualBarrier = false;

        [Header("Player Feedback")]
        [Tooltip("Message shown when player tries to pass")]
        [SerializeField] private string blockedMessage = "⚠️ Complete previous zones to proceed";
        
        [Tooltip("Message color")]
        [SerializeField] private Color messageColor = Color.yellow;

        [Tooltip("Font size")]
        [SerializeField] private int fontSize = 30;

        private bool playerNearby = false;
        private AudioSource audioSource;
        private MeshRenderer meshRenderer;
        private bool isDestroyed = false;

        #region Unity Lifecycle

        private void Awake()
        {
            // Setup collider - will be non-trigger to physically block player
            Collider col = GetComponent<Collider>();
            // Start as solid (non-trigger) to block player
            col.isTrigger = false;

            // Setup audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            // Setup visual representation
            SetupVisual();
        }

        private void Start()
        {
            // Check if barrier should already be removed
            CheckBarrierStatus();
        }

        private void Update()
        {
            if (!isDestroyed)
            {
                CheckBarrierStatus();
            }
        }

        #endregion

        #region Barrier Logic

        /// <summary>
        /// Checks if all required zones are cleansed
        /// </summary>
        private void CheckBarrierStatus()
        {
            if (isDestroyed) return;

            bool allCleansed = AreAllZonesCleansed();

            if (allCleansed && autoDestroy)
            {
                RemoveBarrier();
            }
        }

        /// <summary>
        /// Checks if all required zones are cleansed
        /// </summary>
        private bool AreAllZonesCleansed()
        {
            if (requiredZones == null || requiredZones.Length == 0)
            {
                Debug.LogWarning($"[ZoneProgressionBarrier] {gameObject.name} has no required zones assigned!");
                return true; // No requirements = always open
            }

            foreach (var zone in requiredZones)
            {
                if (zone == null)
                {
                    Debug.LogWarning($"[ZoneProgressionBarrier] {gameObject.name} has null zone reference!");
                    continue;
                }

                if (!zone.IsCleansed())
                {
                    return false; // At least one zone is not cleansed
                }
            }

            return true; // All zones are cleansed
        }

        /// <summary>
        /// Removes the barrier (destroys game object)
        /// </summary>
        private void RemoveBarrier()
        {
            if (isDestroyed) return;
            isDestroyed = true;

            Debug.Log($"[ZoneProgressionBarrier] {gameObject.name} - All zones cleansed, removing barrier!");

            // Spawn destroy VFX
            if (destroyVFX != null)
            {
                Instantiate(destroyVFX, transform.position, Quaternion.identity);
            }

            // Play destroy sound
            if (destroySound != null && audioSource != null)
            {
                // Play sound at position (won't be destroyed with game object)
                AudioSource.PlayClipAtPoint(destroySound, transform.position);
            }

            // Destroy this barrier
            Destroy(gameObject, 0.1f);
        }

        #endregion

        #region Visual Setup

        private void SetupVisual()
        {
            meshRenderer = GetComponent<MeshRenderer>();

            if (showVisualBarrier)
            {
                // Create visual representation if it doesn't exist
                if (meshRenderer == null)
                {
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();
                    MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
                    
                    // Create a simple plane mesh
                    meshFilter.mesh = CreatePlaneMesh();
                }

                // Apply barrier material
                if (barrierMaterial != null && meshRenderer != null)
                {
                    meshRenderer.material = barrierMaterial;
                }

                meshRenderer.enabled = true;
            }
            else
            {
                // Keep invisible
                if (meshRenderer != null)
                    meshRenderer.enabled = false;
            }
        }

        private Mesh CreatePlaneMesh()
        {
            // Simple plane mesh for visualization
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-5, -5, 0),
                new Vector3(5, -5, 0),
                new Vector3(-5, 5, 0),
                new Vector3(5, 5, 0)
            };
            mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateNormals();
            return mesh;
        }

        #endregion

        #region Collision Detection

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                playerNearby = true;
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                playerNearby = true;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                playerNearby = false;
            }
        }

        #endregion

        #region GUI Display

        private void OnGUI()
        {
            // Only show message if player is nearby and barrier is still active
            if (playerNearby && !isDestroyed && !AreAllZonesCleansed())
            {
                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = fontSize,
                    normal = { textColor = messageColor }
                };

                float screenWidth = Screen.width;
                float screenHeight = Screen.height;
                Vector2 labelSize = style.CalcSize(new GUIContent(blockedMessage));
                float labelX = screenWidth * 0.5f - labelSize.x / 2;
                float labelY = screenHeight * 0.5f - labelSize.y / 2;

                GUI.Label(new Rect(labelX, labelY, labelSize.x, labelSize.y), blockedMessage, style);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Manually check status and remove barrier if ready
        /// </summary>
        public void ForceCheck()
        {
            CheckBarrierStatus();
        }

        /// <summary>
        /// Manually remove barrier regardless of zone status
        /// </summary>
        public void ForceRemove()
        {
            RemoveBarrier();
        }

        public bool IsDestroyed() => isDestroyed;
        public bool CanPass() => AreAllZonesCleansed();

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            Gizmos.color = AreAllZonesCleansed() ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(transform.position, transform.localScale);
        }

        #endregion
    }
}