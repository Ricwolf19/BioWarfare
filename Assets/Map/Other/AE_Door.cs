using UnityEngine;
using cowsins;
using BioWarfare.InfectedZones;

namespace Art_Equilibrium
{
    public class AE_Door : MonoBehaviour
    {
        bool trig, open;
        public float smooth = 2.0f;
        public float DoorOpenAngle = 87.0f;
        private Quaternion defaultRot;
        private Quaternion openRot;
        private Vector3 defaultLocalPos;
        private Vector3 targetLocalSlidePos;

        private bool isKeyPressed;
        private InputManager inputManager;

        [Header("Door Type")]
        public bool isSlidingDoor = false;                  // Тумблер: обычная или раздвижная
        public Vector3 slideOffset = new Vector3(1, 0, 0);  // Направление сдвига для раздвижной двери (в локальных координатах)

        [Header("GUI Settings")]
        public string openMessage = "Open E";
        public string closeMessage = "Close E";
        public Font messageFont;
        public int fontSize = 35;
        public Color fontColor = Color.white;
        public Vector2 messagePosition = new Vector2(0.5f, 0.5f);

        [Header("Zone Lock Settings")]
        [Tooltip("Lock this door when an infected zone is active?")]
        public bool lockDuringZoneCapture = false;
        [Tooltip("Reference to the zone that controls this door (optional)")]
        public InfectedZoneController controllingZone;
        [Tooltip("Message shown when door is locked")]
        public string lockedMessage = "⚠️ You need to disinfect the zone!!!";
        [Tooltip("Color for locked message")]
        public Color lockedMessageColor = Color.red;

        private string doorMessage = "";
        private bool isLocked = false;

        [Header("Audio Settings")]
        public AudioClip openSound;
        public AudioClip closeSound;
        private AudioSource audioSource;

        private void Start()
        {
            defaultRot = transform.rotation;
            openRot = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + DoorOpenAngle, transform.eulerAngles.z);
            defaultLocalPos = transform.localPosition;
            targetLocalSlidePos = defaultLocalPos + slideOffset;
            isKeyPressed = false;

            audioSource = gameObject.AddComponent<AudioSource>();
            
            // Find the FPS Engine InputManager using Unity 2023+ API
            inputManager = FindFirstObjectByType<InputManager>();
            if (inputManager == null)
            {
                Debug.LogWarning("[AE_Door] InputManager not found. Door interactions may not work.");
            }

            // Subscribe to zone events if controlling zone is assigned
            if (lockDuringZoneCapture && controllingZone != null)
            {
                SubscribeToZoneEvents();
                UpdateLockState();
            }
        }

        private void Update()
        {
            if (isSlidingDoor)
            {
                Vector3 targetPos = open ? targetLocalSlidePos : defaultLocalPos;
                transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smooth);
            }
            else
            {
                Quaternion targetRot = open ? openRot : defaultRot;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smooth);
            }

            // Update lock state if zone locking is enabled
            if (lockDuringZoneCapture && controllingZone != null)
            {
                UpdateLockState();
            }

            // Use the new Input System via FPS Engine's InputManager
            if (inputManager != null)
            {
                // Only allow interaction if door is not locked
                if (inputManager.StartInteraction && trig && !isKeyPressed && !isLocked)
                {
                    open = !open;
                    isKeyPressed = true;
                    PlayDoorSound();
                }

                if (!inputManager.Interacting)
                {
                    isKeyPressed = false;
                }
            }

            // Update door message based on lock state
            if (isLocked)
            {
                doorMessage = trig ? lockedMessage : "";
            }
            else
            {
                doorMessage = trig ? (open ? closeMessage : openMessage) : "";
            }
        }

        private void OnGUI()
        {
            if (!string.IsNullOrEmpty(doorMessage))
            {
                // Use red color if door is locked, otherwise use normal color
                Color messageColor = isLocked ? lockedMessageColor : fontColor;

                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = fontSize,
                    normal = { textColor = messageColor }
                };

                if (messageFont != null)
                {
                    style.font = messageFont;
                }

                float screenWidth = Screen.width;
                float screenHeight = Screen.height;
                Vector2 labelSize = style.CalcSize(new GUIContent(doorMessage));
                float labelX = screenWidth * messagePosition.x - labelSize.x / 2;
                float labelY = screenHeight * messagePosition.y - labelSize.y / 2;

                GUI.Label(new Rect(labelX, labelY, labelSize.x, labelSize.y), doorMessage, style);
            }
        }

        private void OnTriggerEnter(Collider coll)
        {
            if (coll.CompareTag("Player"))
            {
                doorMessage = open ? closeMessage : openMessage;
                trig = true;
            }
        }

        private void OnTriggerExit(Collider coll)
        {
            if (coll.CompareTag("Player"))
            {
                doorMessage = "";
                trig = false;
            }
        }

        private void PlayDoorSound()
        {
            if (audioSource != null)
            {
                if (open && openSound != null)
                {
                    audioSource.clip = openSound;
                    audioSource.Play();
                }
                else if (!open && closeSound != null)
                {
                    audioSource.clip = closeSound;
                    audioSource.Play();
                }
            }
        }

        #region Zone Lock System

        private void SubscribeToZoneEvents()
        {
            if (controllingZone != null)
            {
                // Listen to zone state changes
                Debug.Log($"[AE_Door] Subscribed to zone events: {controllingZone.GetZoneData()?.zoneName}");
            }
        }

        private void UpdateLockState()
        {
            if (controllingZone == null) return;

            ZoneState state = controllingZone.GetState();

            // Lock door when zone is active, capturing, or pillar is vulnerable
            // Unlock when zone is locked (not started) or cleansed (completed)
            isLocked = (state == ZoneState.Active || 
                       state == ZoneState.Capturing || 
                       state == ZoneState.PillarVulnerable);

            // Force close door if it gets locked while open
            if (isLocked && open)
            {
                open = false;
                Debug.Log($"[AE_Door] Door force-closed due to zone lock");
            }
        }

        /// <summary>
        /// Manually lock/unlock the door (called by external systems)
        /// </summary>
        public void SetLocked(bool locked)
        {
            isLocked = locked;
            if (isLocked && open)
            {
                open = false;
            }
        }

        public bool IsLocked() => isLocked;

        #endregion
    }
}
