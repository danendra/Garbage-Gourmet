using UnityEngine;

namespace TK.Gameplay
{
    public class HandVisual : MonoBehaviour
    {
        [SerializeField] private PlayerMovement player;

        [Header("Tilt Settings")]
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private float maxAngle = 15f;

        private float currentAngle;
        private float angleVelocity;

        void Update()
        {
            if (player == null) return;

            float deltaX = player.GetDeltaX();

            float targetAngle = Mathf.Clamp(deltaX * 6f, -25f, 25f);

            currentAngle = Mathf.SmoothDampAngle(
                currentAngle,
                targetAngle,
                ref angleVelocity,
                0.08f
            );

            transform.localRotation =
                Quaternion.Euler(0f, 0f, currentAngle);
        }
    }
}