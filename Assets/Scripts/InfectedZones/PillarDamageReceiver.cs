using UnityEngine;
using cowsins; // FPS Engine namespace

namespace BioWarfare.InfectedZones
{
    /// <summary>
    /// Receives damage from FPS Engine bullets and forwards to InfectedPillar
    /// IMPLEMENTS IDamageable - Required for FPS Engine bullet system!
    /// Attach this to the pillar GameObject
    /// </summary>
    public class PillarDamageReceiver : MonoBehaviour, IDamageable
    {
        private InfectedPillar pillar;
        
        void Start()
        {
            pillar = GetComponent<InfectedPillar>();
            if (pillar == null)
            {
                Debug.LogError("[PillarDamageReceiver] No InfectedPillar component found!");
            }
            else
            {
                Debug.Log("[PillarDamageReceiver] Successfully linked to InfectedPillar. Ready to receive damage.");
            }
        }
        
        /// <summary>
        /// IDamageable interface implementation - Called by FPS Engine bullets
        /// This is THE method that FPS Engine weapons will call!
        /// </summary>
        public void Damage(float damage, bool isHeadshot)
        {
            if (pillar != null)
            {
                pillar.Damage(damage);
                Debug.Log($"[PillarDamageReceiver] Received {damage} damage from FPS Engine bullet. Forwarding to pillar.");
            }
        }
    }
}