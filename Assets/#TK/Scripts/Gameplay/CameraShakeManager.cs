using UnityEngine;
using Unity.Cinemachine;

namespace TK.Gameplay
{
    public class CameraShakeManager : MonoBehaviour
    {
        public static CameraShakeManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void CameraShake(CinemachineImpulseSource impulseSource, float force = 1f)
        {
            if (impulseSource != null)
            {
                impulseSource.GenerateImpulseWithForce(force);
            }
        }
    }
}
