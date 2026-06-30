using System.Collections;
using UnityEngine;

public class EndSequenceController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CameraMovement cam;
    [SerializeField] private ResultUI resultUI;

    [Header("Item Display")]
    [SerializeField] private SpriteRenderer itemDisplay;
    private Vector3 itemDisplayOriginalScale;

    public IEnumerator PlayEndSequence(bool won)
{
    cam.DisableFollow();
    cam.SnapTo(transform.position);

    // Set the sprite but keep it hidden
    itemDisplay.sprite = GameSession.CollectedSprite;
    itemDisplayOriginalScale = itemDisplay.transform.localScale;
    itemDisplay.transform.localScale = Vector3.zero;

    // Pickup — fade item in
    animator.SetTrigger("Pickup");
    AudioManager.Instance.PlayGrab();
yield return null;
while (animator.IsInTransition(0))
    yield return null;
yield return WaitForAnimationAt("Pickup", 0.3f);
yield return ScaleItem(Vector3.zero, itemDisplayOriginalScale, 0.2f);
yield return WaitForAnimationComplete("Pickup");

    // Eat — shrink item out midway through eat animation
    if (won)
    {
        animator.SetTrigger("EatFood");
        AudioManager.Instance.PlayEatFood();
yield return WaitForAnimationAt("EatFood", 0.1f);
yield return ScaleItem(itemDisplayOriginalScale, Vector3.zero, 0.2f);
yield return WaitForAnimationComplete("EatFood");
    }
    else
    {
        animator.SetTrigger("EatTrash");
        AudioManager.Instance.PlayEatTrash();
yield return WaitForAnimationAt("EatTrash", 0.1f);
yield return ScaleItem(itemDisplayOriginalScale, Vector3.zero, 0.2f);
yield return WaitForAnimationComplete("EatTrash");
    }

    animator.SetTrigger("Rating");
yield return null;
while (animator.IsInTransition(0))
    yield return null;
yield return WaitForAnimationAt("Rating", 0.4f); // ← tune this, fires sound at 50%
AudioManager.Instance.PlayRating();
yield return WaitForAnimationComplete("Rating");
    yield return new WaitForSeconds(0.25f);
    resultUI.Show();
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

private IEnumerator WaitForAnimationHalfway(string stateName)
{
    yield return new WaitUntil(() =>
        animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));

    yield return new WaitUntil(() =>
        animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f);
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
    private IEnumerator PlayAnim(string triggerName)
{
    animator.SetTrigger(triggerName);
    yield return null;

    while (animator.IsInTransition(0))
        yield return null;

    yield return new WaitUntil(() =>
        animator.GetCurrentAnimatorStateInfo(0).IsName(triggerName));

    while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        yield return null;
}
}