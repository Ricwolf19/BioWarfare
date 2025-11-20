using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BioWarfare.Stats;
using BioWarfare.Backend;

namespace BioWarfare.UI
{
    /// <summary>
    /// Controls main menu UI and player name input
    /// Integrates with PlayerStatsTracker and Firebase
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private InputField playerNameInput;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Text statusText;

        [Header("Scene Settings")]
        [SerializeField] private string introSceneName = "IntroScene";
        [SerializeField] private string gameSceneName = "GameScene";
        [SerializeField] private bool useIntroVideo = true;

        [Header("Name Settings")]
        [SerializeField] private int maxNameLength = 20;
        [SerializeField] private string defaultNamePrefix = "Player";

        private void Start()
        {
            // Setup UI listeners
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameClicked);
            }

            if (playerNameInput != null)
            {
                playerNameInput.characterLimit = maxNameLength;
                playerNameInput.onEndEdit.AddListener(OnNameInputChanged);
            }

            // Reset player stats when returning to menu
            PlayerStatsTracker.Instance.ResetStats();

            // Show welcome message
            UpdateStatusText("Enter your name and click Start Game!");

            // Ensure cursor is visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnNameInputChanged(string newName)
        {
            // Sanitize input (remove special characters)
            string sanitized = SanitizePlayerName(newName);
            if (sanitized != newName && playerNameInput != null)
            {
                playerNameInput.text = sanitized;
            }
        }

        private void OnStartGameClicked()
        {
            string playerName = playerNameInput != null ? playerNameInput.text.Trim() : "";

            // Generate name if empty
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = GenerateRandomPlayerName();
                UpdateStatusText($"Name set to: {playerName}");
            }

            // Start game with player name
            StartGame(playerName);
        }

        private void StartGame(string playerName)
        {
            // Initialize player stats tracker
            PlayerStatsTracker.Instance.StartNewGame(playerName);

            UpdateStatusText("Starting game...");

            // Load intro or game scene
            string targetScene = useIntroVideo ? introSceneName : gameSceneName;
            
            if (SceneExists(targetScene))
            {
                SceneManager.LoadScene(targetScene);
            }
            else
            {
                Debug.LogError($"[MainMenuController] Scene '{targetScene}' not found!");
                UpdateStatusText("Error: Scene not found!");
            }
        }

        private string SanitizePlayerName(string name)
        {
            // Remove special characters, keep only letters, numbers, spaces, and basic punctuation
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

        private string GenerateRandomPlayerName()
        {
            int randomNumber = Random.Range(1000, 9999);
            return $"{defaultNamePrefix}{randomNumber}";
        }

        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Debug.Log($"[MainMenuController] {message}");
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
    }
}