using UnityEngine;
using cowsins;
using BioWarfare.Stats;
using BioWarfare.Backend;

namespace BioWarfare.Integration
{
    /// <summary>
    /// Handles player death and uploads stats to Firebase
    /// Attaches to Player GameObject
    /// </summary>
    public class PlayerDeathHandler : MonoBehaviour
    {
        private PlayerStats playerStats;
        private bool hasHandledDeath = false;

        private void Start()
        {
            // Get PlayerStats component
            playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("[PlayerDeathHandler] PlayerStats component not found!");
                return;
            }

            // Subscribe to death event
            playerStats.OnDie += OnPlayerDeath;

            Debug.Log("[PlayerDeathHandler] Subscribed to player death event.");
        }

        private void OnPlayerDeath()
        {
            if (hasHandledDeath) return;
            hasHandledDeath = true;

            Debug.Log("[PlayerDeathHandler] Player died! Ending game and uploading stats...");

            // End game tracking (player died)
            PlayerStatsTracker.Instance.EndGame(died: true);

            // Upload stats to Firebase
            UploadStatsToFirebase();
        }

        private async void UploadStatsToFirebase()
        {
            Debug.Log("[PlayerDeathHandler] Uploading player death stats to Firebase...");

            bool success = await FirebaseManager.Instance.UploadCurrentGameStats();

            if (success)
            {
                Debug.Log("[PlayerDeathHandler] Death stats uploaded successfully!");
            }
            else
            {
                Debug.LogWarning("[PlayerDeathHandler] Failed to upload stats (Firebase may be disabled).");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (playerStats != null)
            {
                playerStats.OnDie -= OnPlayerDeath;
            }
        }
    }
}