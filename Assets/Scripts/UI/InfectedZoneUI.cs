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

        [Header("Settings")]
        [SerializeField] private bool showGlobalProgress = true;
        [SerializeField] private bool showCaptureUI = true;

        private InfectedZoneController currentZone;

        #region Unity Lifecycle

        void Start()
        {
            // Subscribe to global progress manager
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.OnProgressUpdated.AddListener(UpdateGlobalProgress);
                GameProgressManager.Instance.OnAllZonesCleansed.AddListener(OnAllZonesCleansed);
            }

            // Hide capture UI initially
            if (capturePanel != null)
                capturePanel.SetActive(false);

            if (pillarPanel != null)
                pillarPanel.SetActive(false);

            UpdateGlobalProgress(0, 0);
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

            // Access progress using reflection since it's private in PointCapture
            float progress = GetCaptureProgress(currentZone);

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

        /// <summary>
        /// Gets capture progress using reflection (PointCapture.progress is private)
        /// </summary>
        private float GetCaptureProgress(InfectedZoneController zone)
        {
            try
            {
                var progressField = typeof(cowsins.PointCapture).GetField("progress", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (progressField != null)
                {
                    return (float)progressField.GetValue(zone);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[InfectedZoneUI] Could not access progress field: {ex.Message}");
            }
            
            return 0f;
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

            // Subscribe to pillar updates (you'd need to add an event to PillarDamageReceiver)
            StartCoroutine(UpdatePillarHealth(pillar));
        }

        /// <summary>
        /// Hides the pillar UI
        /// </summary>
        public void HidePillarUI()
        {
            if (pillarPanel != null)
                pillarPanel.SetActive(false);
        }

        /// <summary>
        /// Updates pillar health bar
        /// </summary>
        private System.Collections.IEnumerator UpdatePillarHealth(PillarDamageReceiver pillar)
        {
            while (pillar != null && !pillar.IsDestroyed())
            {
                if (pillarHealthBar != null)
                {
                    pillarHealthBar.maxValue = 1f;
                    pillarHealthBar.value = pillar.GetHealthPercent();
                }
                yield return new WaitForSeconds(0.1f);
            }

            HidePillarUI();
        }

        #endregion

        #region Public API

        public void SetCurrentZone(InfectedZoneController zone) => currentZone = zone;
        public InfectedZoneController GetCurrentZone() => currentZone;

        #endregion
    }
}