using UnityEngine;
using cowsins;

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Automatically adds enemy to compass and removes when destroyed
    /// Attach this to enemy prefabs alongside CompassElement
    /// Works with Emerald AI - removes marker when enemy dies
    /// </summary>
    [RequireComponent(typeof(CompassElement))]
    public class EnemyCompassMarker : MonoBehaviour
    {
        [Header("Compass Settings")]
        [Tooltip("Icon shown on compass for this enemy")]
        [SerializeField] private Sprite enemyIcon;

        [Tooltip("Auto-register on spawn?")]
        [SerializeField] private bool registerOnStart = true;

        private CompassElement compassElement;
        private bool isRegistered = false;

        #region Unity Lifecycle

        private void Awake()
        {
            // Get or add CompassElement component
            compassElement = GetComponent<CompassElement>();
            if (compassElement == null)
            {
                compassElement = gameObject.AddComponent<CompassElement>();
            }

            // Set icon if provided
            if (enemyIcon != null)
            {
                compassElement.icon = enemyIcon;
            }
        }

        private void Start()
        {
            if (registerOnStart)
            {
                RegisterToCompass();
            }
        }

        private void OnDestroy()
        {
            // ✅ AUTOMATICALLY removes from compass when enemy dies
            UnregisterFromCompass();
        }

        #endregion

        #region Compass Registration

        /// <summary>
        /// Adds this enemy to the compass
        /// </summary>
        public void RegisterToCompass()
        {
            if (isRegistered) return;

            if (cowsins.Compass.Instance != null && compassElement != null)
            {
                compassElement.Add();
                isRegistered = true;
                Debug.Log($"[EnemyCompassMarker] {gameObject.name} added to compass");
            }
            else
            {
                Debug.LogWarning($"[EnemyCompassMarker] Cannot register - Compass or CompassElement is null");
            }
        }

        /// <summary>
        /// Removes this enemy from the compass
        /// </summary>
        public void UnregisterFromCompass()
        {
            if (!isRegistered) return;

            if (compassElement != null)
            {
                compassElement.Remove();
                isRegistered = false;
                Debug.Log($"[EnemyCompassMarker] {gameObject.name} removed from compass");
            }
        }

        #endregion

        #region Public API

        public bool IsRegistered() => isRegistered;
        
        public void SetIcon(Sprite icon)
        {
            enemyIcon = icon;
            if (compassElement != null)
            {
                compassElement.icon = icon;
            }
        }

        #endregion
    }
}