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

        private const string PREFS_SCREENSHAKE = "Setting_ScreenShake";

        public void CameraShake(CinemachineImpulseSource impulseSource, float force = 1f)
        {
            bool isScreenShakeOn = PlayerPrefs.GetInt(PREFS_SCREENSHAKE, 1) == 1;
            if (isScreenShakeOn && impulseSource != null)
            {
                impulseSource.GenerateImpulseWithForce(force);
            }
        }
    }
}
