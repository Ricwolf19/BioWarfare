/// <summary>
/// State machine for Infected Zone progression
/// </summary>
namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Represents the current state of an infected zone
    /// </summary>
    public enum ZoneState
    {
        Locked,             // Zone not yet accessible
        Active,             // Zone accessible, awaiting player
        Capturing,          // Player is capturing the zone
        PillarVulnerable,   // Capture complete, pillar can be damaged
        Cleansed            // Pillar destroyed, zone secured
    }

    /// <summary>
    /// Events fired during zone state transitions
    /// </summary>
    [System.Serializable]
    public class ZoneEvents
    {
        public UnityEngine.Events.UnityEvent OnZoneActivated;
        public UnityEngine.Events.UnityEvent OnPlayerEntered;
        public UnityEngine.Events.UnityEvent OnCaptureStarted;
        public UnityEngine.Events.UnityEvent OnCaptureCompleted;
        public UnityEngine.Events.UnityEvent OnPillarDestroyed;
        public UnityEngine.Events.UnityEvent OnZoneCleansed;
        public UnityEngine.Events.UnityEvent OnPlayerExited;
    }
}