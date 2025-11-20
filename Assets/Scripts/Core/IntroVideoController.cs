using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

namespace BioWarfare.GameFlow
{
    /// <summary>
    /// Controls intro video playback and transitions to game
    /// Similar to EndingVideoController but for game start
    /// </summary>
    public class IntroVideoController : MonoBehaviour
    {
        [Header("Video Settings")]
        [Tooltip("Video Player component (auto-detected if not assigned)")]
        [SerializeField] private VideoPlayer videoPlayer;

        [Tooltip("Camera to render video on (usually Main Camera)")]
        [SerializeField] private Camera targetCamera;

        [Header("Scene Transition")]
        [Tooltip("Name of game scene to load after video")]
        [SerializeField] private string gameSceneName = "GameScene";

        [Tooltip("Automatically start game when video finishes?")]
        [SerializeField] private bool autoStartGame = true;

        [Tooltip("Delay before loading game (seconds)")]
        [SerializeField] private float loadDelay = 0.5f;

        [Header("Controls")]
        [Tooltip("Allow skipping intro with ESC or SPACE?")]
        [SerializeField] private bool allowSkip = true;

        [Tooltip("Show skip prompt?")]
        [SerializeField] private bool showSkipPrompt = true;

        [Tooltip("Skip prompt text")]
        [SerializeField] private string skipPromptText = "Press ESC or SPACE to skip";

        private bool hasFinished = false;

        #region Unity Lifecycle

        private void Start()
        {
            SetupVideoPlayer();
        }

        private void Update()
        {
            // Allow skipping with ESC or SPACE
            if (allowSkip && !hasFinished)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
                {
                    SkipIntro();
                }
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
                    fontSize = 18,
                    normal = { textColor = new Color(1f, 1f, 1f, 0.7f) }
                };

                float padding = 20f;
                Vector2 textSize = style.CalcSize(new GUIContent(skipPromptText));

                Rect rect = new Rect(
                    Screen.width - textSize.x - padding,
                    Screen.height - textSize.y - padding,
                    textSize.x,
                    textSize.y
                );

                GUI.Label(rect, skipPromptText, style);
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
                    Debug.LogError("[IntroVideoController] No VideoPlayer component found!");
                    // Skip to game if no video
                    LoadGameScene();
                    return;
                }
            }

            // Auto-detect camera if not assigned
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    Debug.LogWarning("[IntroVideoController] No camera assigned and no Main Camera found!");
                }
            }

            // Configure video player for fullscreen playback
            ConfigureVideoPlayer();

            // Subscribe to video finished event
            if (autoStartGame)
            {
                videoPlayer.loopPointReached += OnVideoFinished;
            }

            Debug.Log("[IntroVideoController] Intro video configured for fullscreen playback.");
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

            Debug.Log($"[IntroVideoController] Intro video configured: {videoPlayer.clip?.name}");
        }

        #endregion

        #region Video Playback Control

        private void OnVideoFinished(VideoPlayer vp)
        {
            if (hasFinished) return;
            hasFinished = true;

            Debug.Log("[IntroVideoController] Intro finished, starting game...");
            Invoke(nameof(LoadGameScene), loadDelay);
        }

        private void SkipIntro()
        {
            if (hasFinished) return;
            hasFinished = true;

            Debug.Log("[IntroVideoController] Intro skipped by player.");

            // Stop video
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            LoadGameScene();
        }

        private void LoadGameScene()
        {
            Debug.Log($"[IntroVideoController] Loading game scene: {gameSceneName}");

            // Reset game state
            ResetGameState();

            // Check if scene exists
            if (SceneExists(gameSceneName))
            {
                SceneManager.LoadScene(gameSceneName);
            }
            else
            {
                Debug.LogError($"[IntroVideoController] Scene '{gameSceneName}' not found in build settings!");
            }
        }

        private void ResetGameState()
        {
            // Reset time scale
            Time.timeScale = 1f;

            // Lock cursor for FPS gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Ensure audio is playing
            AudioListener.pause = false;

            Debug.Log("[IntroVideoController] Game state reset for gameplay.");
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
        /// Manually skip to game
        /// </summary>
        public void ForceStartGame()
        {
            SkipIntro();
        }

        #endregion
    }
}