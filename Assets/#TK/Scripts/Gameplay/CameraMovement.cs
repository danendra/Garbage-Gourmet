using UnityEngine;
using System.Collections;

namespace TK.Gameplay
{
    public class CameraMovement : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private Transform player;
        [SerializeField] private float smoothing = 5f;
        [SerializeField] private Vector3 offset;
        [SerializeField] private float xmin;
        [SerializeField] private float xmax;

        [Header("Intro Points")]
        [SerializeField] private Transform bottomView;
        [SerializeField] private Transform topView;

        [Header("Timing")]
        [SerializeField] private float panDuration = 1.2f;
        [SerializeField] private float diveDuration = 0.5f;

        private bool followPlayer = false;

        public IEnumerator PlayIntroPan()
        {
            followPlayer = false;

            transform.position = bottomView.position;

            yield return new WaitForSeconds(1f);

            yield return MoveCam(bottomView.position, topView.position, panDuration);
        }

        public IEnumerator PlayDiveDown()
        {
            Vector3 target = topView.position + Vector3.down * 2f;

            yield return MoveCam(topView.position, target, diveDuration);
        }

        public void EnableFollow()
        {
            followPlayer = true;
        }

        public void SnapToPlayer()
        {
            float x = Mathf.Clamp(player.position.x, xmin, xmax);
            float y = player.position.y + offset.y;

            transform.position = new Vector3(x, y, offset.z);
        }

        IEnumerator MoveCam(Vector3 from, Vector3 to, float duration)
        {
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;

                float p = Mathf.SmoothStep(0f, 1f, t / duration);

                transform.position = Vector3.Lerp(from, to, p);

                yield return null;
            }

            transform.position = to;
        }

        void LateUpdate()
        {
            if (!followPlayer) return;

            float x = Mathf.Clamp(player.position.x, xmin, xmax);
            float y = player.position.y + offset.y;

            Vector3 target =
                new Vector3(x, y, offset.z);

            transform.position = Vector3.Lerp(
                transform.position,
                target,
                smoothing * Time.deltaTime
            );
        }

        public void DisableFollow()
        {
            followPlayer = false;
        }

        public void SnapTo(Vector3 position)
        {
            transform.position = new Vector3(position.x, position.y, offset.z);
        }
    }
}