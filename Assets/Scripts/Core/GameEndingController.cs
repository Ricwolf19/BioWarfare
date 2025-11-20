using UnityEngine;
using UnityEngine.SceneManagement;
using cowsins;
using BioWarfare.Stats;
using BioWarfare.Backend;

namespace BioWarfare.GameFlow
{
    /// <summary>
    /// Handles game ending when player reaches helicopter extraction point
    /// Uses E key interaction instead of PointCapture system
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GameEndingController : MonoBehaviour
    {
        [Header("Game Ending Settings")]
        [Tooltip("Name of the ending scene (with final video)")]
        [SerializeField] private string endingSceneName = "EndingScene";

        [Tooltip("Delay before loading ending scene (seconds)")]
        [SerializeField] private float transitionDelay = 2f;

        [Header("Interaction")]
        [Tooltip("Message shown when player can interact")]
        [SerializeField] private string interactionMessage = "[E] Board Helicopter";

        [Tooltip("Message color")]
        [SerializeField] private Color messageColor = Color.green;

        [Tooltip("Font size")]
        [SerializeField] private int fontSize = 30;

        [Header("Feedback")]
        [Tooltip("Show message when extraction starts?")]
        [SerializeField] private bool showExtractionMessage = true;

        [Tooltip("Extraction complete message")]
        [SerializeField] private string extractionMessage = "🚁 EXTRACTION COMPLETE!";

        [Tooltip("VFX when extraction starts")]
        [SerializeField] private GameObject extractionVFX;

        [Tooltip("Audio when extraction starts")]
        [SerializeField] private AudioClip extractionSound;

        [Header("Optional: Fade to Black")]
        [Tooltip("Fade screen to black before transition?")]
        [SerializeField] private bool useFadeTransition = true;

        [Tooltip("Fade duration (seconds)")]
        [SerializeField] private float fadeDuration = 1.5f;

        private AudioSource audioSource;
        private bool isEnding = false;
        private bool playerNearby = false;
        private InputManager inputManager;

        #region Unity Lifecycle

        private void Start()
        {
            // Setup collider as trigger
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;

            // Setup audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            Debug.Log($"[GameEndingController] Helicopter extraction point initialized. Scene: {endingSceneName}");
        }

        private void Update()
        {
            // Check for E key interaction
            if (playerNearby && !isEnding && inputManager != null)
            {
                if (inputManager.StartInteraction)
                {
                    TriggerExtraction();
                }
            }
        }

        #endregion

        #region Trigger Detection

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerNearby = true;

                // Get InputManager from player
                var playerDeps = other.GetComponent<PlayerDependencies>();
                if (playerDeps != null)
                {
                    inputManager = playerDeps.InputManager;
                }

                Debug.Log("[GameEndingController] Player near helicopter. Press E to board.");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerNearby = false;
                inputManager = null;
            }
        }

        #endregion

        #region Extraction Sequence

        /// <summary>
        /// Triggers the extraction sequence
        /// </summary>
        public void TriggerExtraction()
        {
            if (isEnding) return;
            isEnding = true;

            Debug.Log("[GameEndingController] Extraction triggered! Starting game ending sequence...");

            // End game tracking (player won!)
            PlayerStatsTracker.Instance.EndGame(died: false);

            // Upload stats to Firebase
            UploadStatsToFirebase();

            StartExtractionSequence();
        }

        private void StartExtractionSequence()
        {
            // Play VFX
            if (extractionVFX != null)
            {
                Instantiate(extractionVFX, transform.position, Quaternion.identity);
            }

            // Play sound
            if (extractionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(extractionSound);
            }

            // Show UI notification
            if (showExtractionMessage)
            {
                ShowExtractionNotification();
            }

            // Don't lock controls - scene transition will naturally end player control
            // Start transition after delay
            Invoke(nameof(TransitionToEnding), transitionDelay);
        }

        private void ShowExtractionNotification()
        {
            // Try to show notification via InfectedZoneUI
            var zoneUI = FindAnyObjectByType<InfectedZones.InfectedZoneUI>();
            if (zoneUI != null)
            {
                zoneUI.ShowNotification(extractionMessage);
            }
            else
            {
                Debug.Log($"[GameEndingController] {extractionMessage}");
            }
        }

        #endregion

        #region Scene Transition

        private void TransitionToEnding()
        {
            if (useFadeTransition)
            {
                StartCoroutine(FadeAndLoadScene());
            }
            else
            {
                LoadEndingScene();
            }
        }

        private void LoadEndingScene()
        {
            Debug.Log($"[GameEndingController] Loading ending scene: {endingSceneName}");

            // No need to manipulate input controls
            // Scene change will naturally reset player state
            // New scenes (video/menu) start with fresh input state

            // Check if scene exists in build settings
            if (SceneExists(endingSceneName))
            {
                SceneManager.LoadScene(endingSceneName);
            }
            else
            {
                Debug.LogError($"[GameEndingController] Scene '{endingSceneName}' not found in build settings! Add it in File > Build Settings.");
            }
        }

        private System.Collections.IEnumerator FadeAndLoadScene()
        {
            // Create fade overlay
            GameObject fadeObj = new GameObject("FadeOverlay");
            Canvas canvas = fadeObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            UnityEngine.UI.Image fadeImage = fadeObj.AddComponent<UnityEngine.UI.Image>();
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.rectTransform.anchorMin = Vector2.zero;
            fadeImage.rectTransform.anchorMax = Vector2.one;
            fadeImage.rectTransform.sizeDelta = Vector2.zero;

            // Fade to black
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }

            fadeImage.color = Color.black;

            // Load scene
            LoadEndingScene();
        }

        private bool SceneExists(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneNameInBuild == sceneName)
                    return true;
            }
            return false;
        }

        #endregion

        #region GUI Display

        private void OnGUI()
        {
            // Show interaction prompt when player is nearby
            if (playerNearby && !isEnding)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = fontSize,
                    normal = { textColor = messageColor },
                    wordWrap = false,
                    clipping = TextClipping.Overflow
                };

                float screenWidth = Screen.width;
                float screenHeight = Screen.height;
                
                // Calculate text size with extra padding to prevent clipping
                Vector2 labelSize = style.CalcSize(new GUIContent(interactionMessage));
                
                // Add padding to prevent text clipping (especially vertical)
                float paddingX = 20f;
                float paddingY = fontSize * 0.5f; // Extra vertical space based on font size
                
                float totalWidth = labelSize.x + paddingX;
                float totalHeight = labelSize.y + paddingY;
                
                float labelX = (screenWidth - totalWidth) * 0.5f;
                float labelY = (screenHeight - totalHeight) * 0.5f;

                GUI.Label(new Rect(labelX, labelY, totalWidth, totalHeight), interactionMessage, style);
            }
        }

        #endregion

        #region Firebase Integration

        private async void UploadStatsToFirebase()
        {
            Debug.Log("[GameEndingController] Uploading player stats to Firebase...");

            bool success = await FirebaseManager.Instance.UploadCurrentGameStats();

            if (success)
            {
                Debug.Log("[GameEndingController] Stats uploaded successfully!");
            }
            else
            {
                Debug.LogWarning("[GameEndingController] Failed to upload stats (Firebase may be disabled).");
            }
        }

        #endregion

        #region Public API

        public bool IsEnding() => isEnding;
        public bool IsPlayerNearby() => playerNearby;

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            // Draw helicopter icon/indicator
            Gizmos.color = isEnding ? Color.red : (playerNearby ? Color.yellow : Color.green);
            Gizmos.DrawWireSphere(transform.position, 3f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 5f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, 3f);
        }

        #endregion
    }
}