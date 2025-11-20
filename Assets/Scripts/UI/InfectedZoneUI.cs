using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// UI controller for infected zone system
    /// Displays capture progress, zone info, and global progress
    /// </summary>
    public class InfectedZoneUI : MonoBehaviour
    {
        [Header("Global Progress UI")]
        [SerializeField] private TextMeshProUGUI zonesRemainingText;
        [SerializeField] private TextMeshProUGUI zonesCleansedText;
        [SerializeField] private Slider globalProgressBar;

        [Header("Zone Capture UI")]
        [SerializeField] private GameObject capturePanel;
        [SerializeField] private TextMeshProUGUI zoneNameText;
        [SerializeField] private TextMeshProUGUI capturePromptText;
        [SerializeField] private Slider captureProgressBar;
        [SerializeField] private TextMeshProUGUI capturePercentText;

        [Header("Pillar UI")]
        [SerializeField] private GameObject pillarPanel;
        [SerializeField] private TextMeshProUGUI pillarPromptText;
        [SerializeField] private Slider pillarHealthBar;

        [Header("Notification UI")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private float notificationDuration = 3f;

        [Header("Settings")]
        [SerializeField] private bool showGlobalProgress = true;
        [SerializeField] private bool showCaptureUI = true;

        private InfectedZoneController currentZone;
        private Coroutine notificationCoroutine;
        private Coroutine pillarHealthCoroutine;

        #region Unity Lifecycle

        void Start()
        {
            // Subscribe to global progress manager
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.OnProgressUpdated.AddListener(UpdateGlobalProgress);
                GameProgressManager.Instance.OnAllZonesCleansed.AddListener(OnAllZonesCleansed);
                
                // Delay initialization to let zones register first
                StartCoroutine(InitializeAfterZones());
            }
            else
            {
                Debug.LogWarning("[InfectedZoneUI] GameProgressManager not found! Global progress will not work.");
                UpdateGlobalProgress(0, 5); // Show 0/5 as fallback
            }

            // Hide capture UI initially
            if (capturePanel != null)
                capturePanel.SetActive(false);

            if (pillarPanel != null)
                pillarPanel.SetActive(false);
            
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }

        void Update()
        {
            // Update capture progress if player is in a zone
            if (currentZone != null && showCaptureUI)
            {
            UpdateCaptureProgress();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Wait for zones to register before initializing UI
        /// </summary>
        private System.Collections.IEnumerator InitializeAfterZones()
        {
            // Wait for next frame to let zones register
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.2f);
            
            if (GameProgressManager.Instance != null)
            {
                int totalZones = GameProgressManager.Instance.GetTotalZones();
                int cleansedZones = GameProgressManager.Instance.GetCleansedZones();
                
                // If still 0 zones registered, it means we need to wait longer or there's an issue
                if (totalZones == 0)
                {
                    Debug.LogWarning("[InfectedZoneUI] No zones registered yet! Waiting...");
                    yield return new WaitForSeconds(0.3f);
                    totalZones = GameProgressManager.Instance.GetTotalZones();
                    cleansedZones = GameProgressManager.Instance.GetCleansedZones();
                }
                
                UpdateGlobalProgress(cleansedZones, totalZones);
                Debug.Log($"[InfectedZoneUI] Initialized with {cleansedZones}/{totalZones} zones");

                // Show the first zone name hint if available
                if (totalZones > 0 && zoneNameText != null)
                {
                    string firstZoneName = GameProgressManager.Instance.GetCurrentZoneName();
                    if (!string.IsNullOrEmpty(firstZoneName) && firstZoneName != "None")
                    {
                        Debug.Log($"[InfectedZoneUI] First zone available: {firstZoneName}");
                    }
                }
            }
        }

        #endregion

        #region Global Progress

        /// <summary>
        /// Updates the global zone progress display
        /// </summary>
        public void UpdateGlobalProgress(int cleansed, int total)
        {
            if (!showGlobalProgress) return;

            int remaining = total - cleansed;

            if (zonesRemainingText != null)
                zonesRemainingText.text = $"Zones Remaining: {remaining}";

            if (zonesCleansedText != null)
                zonesCleansedText.text = $"Zones Cleansed: {cleansed}/{total}";

            if (globalProgressBar != null)
            {
                globalProgressBar.maxValue = total;
                globalProgressBar.value = cleansed;
            }
        }

        private void OnAllZonesCleansed()
        {
            if (zonesRemainingText != null)
                zonesRemainingText.text = "ALL ZONES SECURED!";

            if (zonesCleansedText != null)
                zonesCleansedText.text = "MISSION COMPLETE";
        }

        #endregion

        #region Zone Capture UI

        /// <summary>
        /// Shows the zone capture UI when player enters a zone
        /// </summary>
        public void ShowZoneCaptureUI(InfectedZoneController zone)
        {
            currentZone = zone;

            if (capturePanel != null)
                capturePanel.SetActive(true);

            if (zoneNameText != null && zone.GetZoneData() != null)
                zoneNameText.text = zone.GetZoneData().zoneName;

            if (capturePromptText != null)
                capturePromptText.text = "Hold position to capture zone...";
        }

        /// <summary>
        /// Hides the zone capture UI
        /// </summary>
        public void HideZoneCaptureUI()
        {
            currentZone = null;

            if (capturePanel != null)
                capturePanel.SetActive(false);
        }

        /// <summary>
        /// Updates the capture progress bar
        /// </summary>
        private void UpdateCaptureProgress()
        {
            if (currentZone == null) return;

            // Get progress from ZoneCaptureSystem
            float progress = currentZone.GetCaptureProgress();

            if (captureProgressBar != null)
            {
                captureProgressBar.maxValue = 100f;
                captureProgressBar.value = progress;
            }

            if (capturePercentText != null)
                capturePercentText.text = $"{Mathf.FloorToInt(progress)}%";

            // Update prompt based on state
            if (capturePromptText != null)
            {
                var state = currentZone.GetState();
                capturePromptText.text = state switch
                {
                    ZoneState.Capturing => "Capturing zone...",
                    ZoneState.PillarVulnerable => "DESTROY THE PILLAR!",
                    _ => "Hold position to capture"
                };
            }
        }

        #endregion

        #region Pillar UI

        /// <summary>
        /// Shows the pillar health UI
        /// </summary>
        public void ShowPillarUI(PillarDamageReceiver pillar)
        {
            if (pillarPanel != null)
                pillarPanel.SetActive(true);

            if (pillarPromptText != null)
                pillarPromptText.text = "Destroy the Infection Pillar!";

            // Set pillar health bar color to purple
            if (pillarHealthBar != null)
            {
                var fillImage = pillarHealthBar.fillRect?.GetComponent<UnityEngine.UI.Image>();
                if (fillImage != null)
                {
                    fillImage.color = new Color(0.6f, 0.2f, 1f); // Purple color
                    Debug.Log("[InfectedZoneUI] Pillar health bar color set to purple");
                }
            }

            // Stop previous coroutine if running
            if (pillarHealthCoroutine != null)
            {
                StopCoroutine(pillarHealthCoroutine);
            }

            // Start updating pillar health
            pillarHealthCoroutine = StartCoroutine(UpdatePillarHealth(pillar));
        }

        /// <summary>
        /// Hides the pillar UI
        /// </summary>
        public void HidePillarUI()
        {
            // Stop the health update coroutine
            if (pillarHealthCoroutine != null)
            {
                StopCoroutine(pillarHealthCoroutine);
                pillarHealthCoroutine = null;
            }

            if (pillarPanel != null)
                pillarPanel.SetActive(false);
        }

        /// <summary>
        /// Updates pillar health bar continuously
        /// </summary>
        private System.Collections.IEnumerator UpdatePillarHealth(PillarDamageReceiver pillar)
        {
            if (pillar == null)
            {
                Debug.LogWarning("[InfectedZoneUI] Pillar is null, cannot update health bar");
                yield break;
            }

            Debug.Log($"[InfectedZoneUI] Started pillar health tracking");

            while (pillar != null && !pillar.IsDestroyed())
            {
                if (pillarHealthBar != null)
                {
                    float healthPercent = pillar.GetHealthPercent();
                    pillarHealthBar.maxValue = 1f;
                    pillarHealthBar.value = healthPercent;
                    
                    // Debug every second to track updates
                    if (Time.frameCount % 60 == 0)
                    {
                        Debug.Log($"[InfectedZoneUI] Pillar health: {healthPercent * 100:F1}%");
                    }
                }
                
                // Update every frame for smooth visual feedback
                yield return null;
            }

            Debug.Log("[InfectedZoneUI] Pillar destroyed, hiding UI");
            HidePillarUI();
        }

        #endregion

        #region Notification System

        /// <summary>
        /// Shows a temporary notification message to the player
        /// </summary>
        public void ShowNotification(string message)
        {
            if (notificationPanel == null || notificationText == null)
            {
                Debug.LogWarning("[InfectedZoneUI] Notification panel not assigned!");
                return;
            }

            // Stop previous notification if active
            if (notificationCoroutine != null)
                StopCoroutine(notificationCoroutine);

            notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message));
        }

        private System.Collections.IEnumerator ShowNotificationCoroutine(string message)
        {
            // Show notification
            notificationPanel.SetActive(true);
            notificationText.text = message;

            // Optional: Fade in animation
            CanvasGroup canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                float fadeInTime = 0.3f;
                float elapsed = 0f;

                while (elapsed < fadeInTime)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
                    yield return null;
                }
                canvasGroup.alpha = 1f;
            }

            // Wait for duration
            yield return new WaitForSeconds(notificationDuration);

            // Optional: Fade out animation
            if (canvasGroup != null)
            {
                float fadeOutTime = 0.3f;
                float elapsed = 0f;

                while (elapsed < fadeOutTime)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
                    yield return null;
                }
            }

            // Hide notification
            notificationPanel.SetActive(false);
        }

        #endregion

        #region Public API

        public void SetCurrentZone(InfectedZoneController zone) => currentZone = zone;
        public InfectedZoneController GetCurrentZone() => currentZone;

        #endregion
    }
}