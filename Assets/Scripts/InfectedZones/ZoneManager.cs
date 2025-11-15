using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Zone Manager - Controls progression through infected zones
    /// Ricardo Tapia - UTCH 2025
    /// </summary>
    public class ZoneManager : MonoBehaviour
    {
        [Header("Zones")]
        [Tooltip("All infected zones in the level (in order)")]
        public List<InfectedZone> zones = new List<InfectedZone>();
        
        [Header("UI")]
        public TextMeshProUGUI zoneNameText;
        public TextMeshProUGUI objectiveText;
        public Slider progressSlider;
        public GameObject zoneCompletePanel;
        
        [Header("Settings")]
        [Tooltip("Auto-activate next zone when current completes")]
        public bool autoProgressToNextZone = true;
        
        private int currentZoneIndex = 0;
        private InfectedZone currentZone;
        
        void Start()
        {
            // Hide complete panel
            if (zoneCompletePanel) zoneCompletePanel.SetActive(false);
            
            // Subscribe to zone events
            if (zones != null && zones.Count > 0)
            {
                foreach (var zone in zones)
                {
                    if (zone != null)
                    {
                        zone.OnZoneCompleted.AddListener(() => OnZoneCompleted(zone));
                    }
                }
            }
            
            UpdateUI();
        }
        
        void Update()
        {
            UpdateUI();
        }
        
        /// <summary>
        /// Called when a zone is completed
        /// </summary>
        private void OnZoneCompleted(InfectedZone zone)
        {
            Debug.Log($"[ZoneManager] Zone '{zone.zoneName}' completed!");
            
            // Show completion UI
            if (zoneCompletePanel)
            {
                zoneCompletePanel.SetActive(true);
                Invoke(nameof(HideCompletePanel), 3f);
            }
            
            // Progress to next zone
            if (autoProgressToNextZone)
            {
                Invoke(nameof(ActivateNextZone), 3f);
            }
        }
        
        /// <summary>
        /// Activates the next zone in sequence
        /// </summary>
        public void ActivateNextZone()
        {
            currentZoneIndex++;
            
            if (currentZoneIndex < zones.Count)
            {
                currentZone = zones[currentZoneIndex];
                Debug.Log($"[ZoneManager] Next zone: {currentZone.zoneName}");
            }
            else
            {
                Debug.Log("<color=cyan>[ZoneManager]</color> ALL ZONES COMPLETED! 🎉");
                // TODO: Trigger level completion or next level load
            }
        }
        
        /// <summary>
        /// Updates UI with current zone info
        /// </summary>
        private void UpdateUI()
        {
            // Check if zones list is valid
            if (zones == null || zones.Count == 0 || currentZoneIndex >= zones.Count) return;
            
            currentZone = zones[currentZoneIndex];
            if (currentZone == null) return;
            
            // Zone name
            if (zoneNameText)
            {
                zoneNameText.text = currentZone.IsActive() ? currentZone.zoneName : "Explore...";
            }
            
            // Objective text
            if (objectiveText)
            {
                if (!currentZone.IsActive())
                {
                    objectiveText.text = "Find infected zone";
                }
                else if (currentZone.IsCleared())
                {
                    objectiveText.text = "Zone Cleared!";
                }
                else
                {
                    switch (currentZone.zoneType)
                    {
                        case InfectedZone.ZoneType.DestroyPillars:
                            objectiveText.text = "Destroy all pillars";
                            break;
                        case InfectedZone.ZoneType.CapturePoint:
                            objectiveText.text = "Capture the zone";
                            break;
                        case InfectedZone.ZoneType.KillAllEnemies:
                            objectiveText.text = $"Eliminate enemies ({currentZone.GetEnemiesAlive()} remaining)";
                            break;
                    }
                }
            }
            
            // Progress bar
            if (progressSlider)
            {
                progressSlider.value = currentZone.GetCompletionPercent();
            }
        }
        
        private void HideCompletePanel()
        {
            if (zoneCompletePanel) zoneCompletePanel.SetActive(false);
        }
        
        // Public API
        public InfectedZone GetCurrentZone() => currentZone;
        public int GetCurrentZoneIndex() => currentZoneIndex;
        public int GetTotalZones() => zones.Count;
    }
}