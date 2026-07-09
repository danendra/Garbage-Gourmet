using System.Collections;
using UnityEngine;
using TK.Audio;

namespace TK.Gameplay
{
    using UI;

    public class EndSequenceController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private ResultController resultController;

        [Header("Item Display")]
        [SerializeField] private SpriteRenderer itemDisplay;
        private Vector3 itemDisplayOriginalScale;

        public IEnumerator PlayEndSequence(bool won)
        {
            // Set the sprite but keep it hidden            
            itemDisplayOriginalScale = itemDisplay.transform.localScale;
            itemDisplay.transform.localScale = Vector3.zero;

            yield return new WaitForSeconds(0.1f);

            animator.SetTrigger("Rating");
            yield return null;
            while (animator.IsInTransition(0))
                yield return null;
            yield return WaitForAnimationAt("Rating", 0.4f); // ← tune this, fires sound at 50%
            AudioManager.Instance.PlayRating();
            yield return WaitForAnimationComplete("Rating");
            yield return new WaitForSeconds(0.25f);

            resultController.PlayResult();
        }

        private IEnumerator ScaleItem(Vector3 from, Vector3 to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                itemDisplay.transform.localScale = Vector3.Lerp(from, to, t / duration);
                yield return null;
            }
            itemDisplay.transform.localScale = to;
        }

        private IEnumerator WaitForAnimationComplete(string stateName)
        {
            // Only wait to enter the state if we're not already in it
            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));

            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
        }

        private IEnumerator WaitForAnimationAt(string stateName, float point)
        {
            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));

            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= point);
        }
    }
}