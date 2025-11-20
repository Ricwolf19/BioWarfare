using UnityEngine;
using UnityEngine.UI;
using TMPro;
using cowsins;
using BioWarfare.Stats;

namespace BioWarfare.UI
{
    /// <summary>
    /// Custom Play button that integrates CowsinsButton with MainMenuController
    /// Handles player name input and game start logic
    /// </summary>
    [RequireComponent(typeof(CowsinsButton))]
    public class BioWarfarePlayButton : MonoBehaviour
    {
        [Header("Player Name Input")]
        [Tooltip("Input field for player name")]
        [SerializeField] private TMP_InputField playerNameInput;

        [Header("Scene Flow")]
        [Tooltip("Load intro video first?")]
        [SerializeField] private bool useIntroVideo = true;

        [Tooltip("Name of intro scene")]
        [SerializeField] private string introSceneName = "IntroScene";

        [Tooltip("Name of game scene (if no intro)")]
        [SerializeField] private string gameSceneName = "GameScene";

        [Header("Name Generation")]
        [Tooltip("Max characters for player name")]
        [SerializeField] private int maxNameLength = 20;

        [Tooltip("Prefix for auto-generated names")]
        [SerializeField] private string defaultNamePrefix = "Player";

        [Header("Feedback")]
        [Tooltip("Optional status text to show messages")]
        [SerializeField] private TextMeshProUGUI statusText;

        private CowsinsButton cowsinsButton;

        private void Awake()
        {
            cowsinsButton = GetComponent<CowsinsButton>();
            
            if (cowsinsButton == null)
            {
                Debug.LogError("[BioWarfarePlayButton] CowsinsButton component not found!");
                return;
            }

            // Subscribe to button click AFTER CowsinsButton adds its listener
            cowsinsButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void Start()
        {
            // Setup input field character limit
            if (playerNameInput != null)
            {
                playerNameInput.characterLimit = maxNameLength;
            }

            // Show welcome message
            UpdateStatusText("Enter your name and click Play!");

            // Ensure cursor is visible for menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Called when the Play button is clicked
        /// </summary>
        private void OnPlayButtonClicked()
        {
            string playerName = GetPlayerName();

            // Validate and sanitize name
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = GenerateRandomPlayerName();
                UpdateStatusText($"Name set to: {playerName}");
            }
            else
            {
                playerName = SanitizePlayerName(playerName);
            }

            // Initialize player stats tracker with name
            PlayerStatsTracker.Instance.StartNewGame(playerName);
            
            UpdateStatusText("Starting game...");

            Debug.Log($"[BioWarfarePlayButton] Starting game for player: {playerName}");

            // Note: Scene loading is handled by CowsinsButton's SceneTransition
            // Make sure your CowsinsButton has the correct scene set!
        }

        /// <summary>
        /// Get player name from input field
        /// </summary>
        private string GetPlayerName()
        {
            if (playerNameInput == null)
            {
                Debug.LogWarning("[BioWarfarePlayButton] Player name input not assigned!");
                return string.Empty;
            }

            return playerNameInput.text.Trim();
        }

        /// <summary>
        /// Remove special characters from name
        /// </summary>
        private string SanitizePlayerName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            string sanitized = "";
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == ' ' || c == '_' || c == '-')
                {
                    sanitized += c;
                }
            }
            return sanitized.Trim();
        }

        /// <summary>
        /// Generate random player name
        /// </summary>
        private string GenerateRandomPlayerName()
        {
            int randomNumber = Random.Range(1000, 9999);
            return $"{defaultNamePrefix}{randomNumber}";
        }

        /// <summary>
        /// Update status text if available
        /// </summary>
        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Debug.Log($"[BioWarfarePlayButton] {message}");
        }

        private void OnDestroy()
        {
            // Cleanup listener
            if (cowsinsButton != null)
            {
                cowsinsButton.onClick.RemoveListener(OnPlayButtonClicked);
            }
        }
    }
}