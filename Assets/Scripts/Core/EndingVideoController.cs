using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

namespace BioWarfare.GameFlow
{
    /// <summary>
    /// Controls the ending video playback and transitions to main menu
    /// Ensures video plays fullscreen and auto-returns to menu when finished
    /// </summary>
    public class EndingVideoController : MonoBehaviour
    {
        [Header("Video Settings")]
        [Tooltip("Video Player component (auto-detected if not assigned)")]
        [SerializeField] private VideoPlayer videoPlayer;

        [Tooltip("Camera to render video on (usually Main Camera)")]
        [SerializeField] private Camera targetCamera;

        [Header("Scene Transition")]
        [Tooltip("Name of main menu scene to load after video")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Tooltip("Automatically return to menu when video finishes?")]
        [SerializeField] private bool autoReturnToMenu = true;

        [Tooltip("Delay before returning to menu (seconds)")]
        [SerializeField] private float returnDelay = 0.5f;

        [Header("Controls")]
        [Tooltip("Allow skipping video with ESC key?")]
        [SerializeField] private bool allowSkip = true;

        [Tooltip("Show skip prompt?")]
        [SerializeField] private bool showSkipPrompt = true;

        private bool hasFinished = false;

        #region Unity Lifecycle

        private void Start()
        {
            SetupVideoPlayer();
        }

        private void Update()
        {
            // Allow skipping video with ESC key
            if (allowSkip && !hasFinished && Input.GetKeyDown(KeyCode.Escape))
            {
                SkipVideo();
            }
        }

        private void OnGUI()
        {
            // Show skip prompt
            if (showSkipPrompt && !hasFinished)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.LowerRight,
                    fontSize = 20,
                    normal = { textColor = new Color(1f, 1f, 1f, 0.7f) }
                };

                string skipText = "Press ESC to skip";
                float padding = 20f;
                Vector2 textSize = style.CalcSize(new GUIContent(skipText));
                
                Rect rect = new Rect(
                    Screen.width - textSize.x - padding,
                    Screen.height - textSize.y - padding,
                    textSize.x,
                    textSize.y
                );

                GUI.Label(rect, skipText, style);
            }
        }

        #endregion

        #region Video Setup

        private void SetupVideoPlayer()
        {
            // Auto-detect VideoPlayer if not assigned
            if (videoPlayer == null)
            {
                videoPlayer = GetComponent<VideoPlayer>();
                if (videoPlayer == null)
                {
                    Debug.LogError("[EndingVideoController] No VideoPlayer component found!");
                    return;
                }
            }

            // Auto-detect camera if not assigned
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    Debug.LogWarning("[EndingVideoController] No camera assigned and no Main Camera found!");
                }
            }

            // Configure video player for fullscreen playback
            ConfigureVideoPlayer();

            // Subscribe to video finished event
            if (autoReturnToMenu)
            {
                videoPlayer.loopPointReached += OnVideoFinished;
            }

            Debug.Log("[EndingVideoController] Video player configured for fullscreen playback.");
        }

        private void ConfigureVideoPlayer()
        {
            if (videoPlayer == null) return;

            // Set render mode to Camera Far Plane for fullscreen
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            videoPlayer.targetCamera = targetCamera;

            // Ensure video plays once
            videoPlayer.isLooping = false;

            // Ensure video plays on awake
            videoPlayer.playOnAwake = true;
            videoPlayer.waitForFirstFrame = true;

            // Set aspect ratio to fit screen
            videoPlayer.aspectRatio = VideoAspectRatio.FitVertically;

            Debug.Log($"[EndingVideoController] Video configured: {videoPlayer.clip?.name}");
        }

        #endregion

        #region Video Playback Control

        private void OnVideoFinished(VideoPlayer vp)
        {
            if (hasFinished) return;
            hasFinished = true;

            Debug.Log("[EndingVideoController] Video finished, returning to main menu...");
            Invoke(nameof(ReturnToMainMenu), returnDelay);
        }

        private void SkipVideo()
        {
            if (hasFinished) return;
            hasFinished = true;

            Debug.Log("[EndingVideoController] Video skipped by player.");
            
            // Stop video
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            ReturnToMainMenu();
        }

        private void ReturnToMainMenu()
        {
            Debug.Log($"[EndingVideoController] Loading scene: {mainMenuSceneName}");

            // Reset game state before loading menu
            ResetGameState();

            // Check if scene exists
            if (SceneExists(mainMenuSceneName))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                Debug.LogError($"[EndingVideoController] Scene '{mainMenuSceneName}' not found in build settings!");
            }
        }

        /// <summary>
        /// Resets game state to ensure clean menu/new game start
        /// Does NOT touch input controls - only resets game state
        /// </summary>
        private void ResetGameState()
        {
            // Reset time scale (in case it was paused)
            Time.timeScale = 1f;

            // Unlock and show cursor for menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Reset audio listener pause state
            AudioListener.pause = false;

            Debug.Log("[EndingVideoController] Game state reset for menu.");
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

        #region Public API

        /// <summary>
        /// Manually skip to main menu
        /// </summary>
        public void ForceReturnToMenu()
        {
            SkipVideo();
        }

        #endregion
    }
}