using UnityEngine;
using UnityEngine.Events;

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Handles zone capture progress and timer
    /// Standalone system - no dependencies on PointCapture
    /// </summary>
    public class ZoneCaptureSystem : MonoBehaviour
    {
        [Header("Capture Settings")]
        [SerializeField] private float captureTime = 30f;

        [Header("State")]
        [SerializeField] private float currentProgress = 0f; // 0 to 100
        [SerializeField] private bool isCapturing = false;
        [SerializeField] private bool isCaptured = false;
        [SerializeField] private bool playerInZone = false;

        [Header("Events")]
        public UnityEvent OnCaptureStarted;
        public UnityEvent OnCaptureCompleted;
        public UnityEvent<float> OnProgressChanged;

        private float captureSpeed;

        private void Awake()
        {
            captureSpeed = 100f / captureTime;
        }

        private void Update()
        {
            if (isCaptured) return;

            // Only increase progress when player is in zone and capturing
            if (playerInZone && isCapturing)
            {
                currentProgress += captureSpeed * Time.deltaTime;
                currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
                OnProgressChanged?.Invoke(currentProgress);

                if (currentProgress >= 100f && !isCaptured)
                {
                    CompleteCapture();
                }
            }
            // Progress is PRESERVED when player exits - no decrease
        }

        public void StartCapture()
        {
            if (isCaptured) return;

            playerInZone = true;

            // Only invoke OnCaptureStarted the FIRST time we start capturing
            if (!isCapturing)
            {
                isCapturing = true;
                OnCaptureStarted?.Invoke();
                Debug.Log($"[CaptureSystem] Capture started (Progress: {currentProgress:F1}%)");
            }
            else
            {
                // Re-entering zone - continue from current progress
                Debug.Log($"[CaptureSystem] Resuming capture (Progress: {currentProgress:F1}%)");
            }
        }

        public void StopCapture()
        {
            playerInZone = false;
            
            // DON'T reset isCapturing - this preserves the "started" state
            // so that re-entry will resume instead of restart
            Debug.Log($"[CaptureSystem] Player left zone. Progress preserved: {currentProgress:F1}%");
        }

        private void CompleteCapture()
        {
            isCaptured = true;
            isCapturing = false;
            currentProgress = 100f;
            Debug.Log($"[CaptureSystem] Capture completed!");
            OnCaptureCompleted?.Invoke();
        }

        public void ResetCapture()
        {
            currentProgress = 0f;
            isCaptured = false;
            isCapturing = false;
            playerInZone = false;
        }

        // Getters
        public float GetProgress() => currentProgress;
        public bool IsCapturing() => isCapturing;
        public bool IsCaptured() => isCaptured;
        public bool IsPlayerInZone() => playerInZone;

        // Configuration
        public void SetCaptureTime(float time)
        {
            captureTime = time;
            captureSpeed = 100f / captureTime;
        }
    }
}