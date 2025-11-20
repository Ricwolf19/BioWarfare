using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Singleton manager for tracking global zone progress
    /// Manages zone activation sequence based on zone order
    /// </summary>
    public class GameProgressManager : MonoBehaviour
    {
        public static GameProgressManager Instance { get; private set; }

        [Header("Zone Management")]
        [SerializeField] private List<InfectedZoneController> allZones = new List<InfectedZoneController>();
        [SerializeField] private List<InfectedZoneController> sortedZones = new List<InfectedZoneController>();
        [SerializeField] private int totalZones = 0;
        [SerializeField] private int cleansedZones = 0;
        [SerializeField] private int currentZoneIndex = 0; // Which zone is currently active

        [Header("Events")]
        public UnityEvent<int, int> OnProgressUpdated; // (cleansed, total)
        public UnityEvent<string> OnNextZoneActivated; // Zone name
        public UnityEvent OnAllZonesCleansed;

        #region Singleton

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        #endregion

        #region Initialization

        private void Start()
        {
            // Wait a frame for all zones to register
            Invoke(nameof(InitializeZoneSequence), 0.1f);
        }

        private void InitializeZoneSequence()
        {
            if (allZones.Count == 0)
            {
                Debug.LogWarning("[ProgressManager] No zones registered!");
                return;
            }

            // Sort zones by their order number
            sortedZones = allZones.OrderBy(z => z.GetZoneData().zoneOrder).ToList();
            
            Debug.Log($"[ProgressManager] Activating ALL {sortedZones.Count} zones:");
            
            // ACTIVATE ALL ZONES from the start
            for (int i = 0; i < sortedZones.Count; i++)
            {
                var zone = sortedZones[i];
                Debug.Log($"  {i + 1}. Activating {zone.GetZoneData().zoneName}");
                zone.ActivateFromManager();
                zone.ShowCheckpointMarker(true); // Show all checkpoint markers
            }
            
            Debug.Log($"[ProgressManager] All zones are now active. Enemies will spawn when player enters each zone.");
        }

        #endregion

        #region Zone Registration

        /// <summary>
        /// Registers a zone with the manager
        /// </summary>
        public void RegisterZone(InfectedZoneController zone)
        {
            if (!allZones.Contains(zone))
            {
                allZones.Add(zone);
                totalZones = allZones.Count;
                Debug.Log($"[ProgressManager] Registered zone: {zone.GetZoneData().zoneName} (Order: {zone.GetZoneData().zoneOrder}, Total: {totalZones})");
            }
        }

        #endregion

        #region Zone Activation

        private void ActivateZoneAtIndex(int index)
        {
            if (index < 0 || index >= sortedZones.Count)
            {
                Debug.LogWarning($"[ProgressManager] Invalid zone index: {index}");
                return;
            }

            InfectedZoneController zone = sortedZones[index];
            currentZoneIndex = index;

            Debug.Log($"[ProgressManager] Activating zone {index + 1}/{sortedZones.Count}: {zone.GetZoneData().zoneName}");
            
            zone.ActivateFromManager();
            OnNextZoneActivated?.Invoke(zone.GetZoneData().zoneName);

            // Show checkpoint marker for this zone
            zone.ShowCheckpointMarker(true);
        }

        private void ActivateNextZone()
        {
            int nextIndex = currentZoneIndex + 1;
            
            if (nextIndex < sortedZones.Count)
            {
                Debug.Log($"[ProgressManager] Moving to next zone...");
                ActivateZoneAtIndex(nextIndex);
            }
            else
            {
                Debug.Log($"[ProgressManager] No more zones to activate");
            }
        }

        #endregion

        #region Progress Tracking

        /// <summary>
        /// Called when a zone is cleansed (capture completed)
        /// </summary>
        public void OnZoneCleansed(InfectedZoneController zone)
        {
            cleansedZones++;
            Debug.Log($"[ProgressManager] ✅ Zone cleansed: {zone.GetZoneData().zoneName} ({cleansedZones}/{totalZones})");

            UpdateUI();
            
            // Check if all zones complete
            if (cleansedZones >= totalZones)
            {
                OnAllZonesCompleted();
            }
        }

        private void UpdateUI()
        {
            OnProgressUpdated?.Invoke(cleansedZones, totalZones);
        }

        private void OnAllZonesCompleted()
        {
            Debug.Log("[ProgressManager] ★★★ ALL ZONES CLEANSED! MISSION COMPLETE! ★★★");
            OnAllZonesCleansed?.Invoke();
        }

        #endregion

        #region Public API

        public int GetCleansedCount() => cleansedZones;
        public int GetCleansedZones() => cleansedZones; // Alias for UI compatibility
        public int GetTotalCount() => totalZones;
        public int GetTotalZones() => totalZones; // Alias for UI compatibility
        public float GetProgressPercent() => totalZones > 0 ? (float)cleansedZones / totalZones : 0f;
        public int GetRemainingZones() => totalZones - cleansedZones;
        public string GetCurrentZoneName() => currentZoneIndex < sortedZones.Count ? sortedZones[currentZoneIndex].GetZoneData().zoneName : "None";

        #endregion
    }
}