using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class TransitionAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public IEnumerator PlaySequence(string triggerName, bool canSkip)
{
    animator.SetTrigger(triggerName);

    yield return null;

    // wait until animator leaves transition
    while (animator.IsInTransition(0))
        yield return null;

    // wait until animation completes
    while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
    {
        if (canSkip && WasTapped())
            yield break;

        yield return null;
    }
}

    bool WasTapped()
    {
        return Touchscreen.current != null &&
               Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
    }
}