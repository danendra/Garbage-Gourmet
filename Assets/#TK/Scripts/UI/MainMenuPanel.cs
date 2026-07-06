using UnityEngine;
using DG.Tweening;

namespace TK.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MainMenuPanel : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] protected float transitionDuration = 0.3f;
        [SerializeField] protected Ease showEase = Ease.OutBack;
        [SerializeField] protected Ease hideEase = Ease.InQuad;
        [SerializeField] protected bool scaleTransition = true;
        [SerializeField] protected float minScale = 0.9f;

        protected CanvasGroup canvasGroup;
        protected RectTransform rectTransform;
        protected Tween transitionTween;

        public bool IsShown { get; private set; }

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
        }

        public virtual void Show(bool immediate = false)
        {
            IsShown = true;
            gameObject.SetActive(true);

            if (transitionTween != null) transitionTween.Kill();

            if (immediate)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                if (scaleTransition && rectTransform != null)
                {
                    rectTransform.localScale = Vector3.one;
                }
                OnShown();
                return;
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            // Play click sound when opening
            if (TK.Audio.AudioManager.Instance != null)
            {
                TK.Audio.AudioManager.Instance.PlayButtonClick();
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(1f, transitionDuration).From(0f));

            if (scaleTransition && rectTransform != null)
            {
                seq.Join(rectTransform.DOScale(1f, transitionDuration).From(minScale).SetEase(showEase));
            }

            seq.OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                OnShown();
            });

            transitionTween = seq;
        }

        public virtual void Hide(bool immediate = false)
        {
            IsShown = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            if (transitionTween != null) transitionTween.Kill();

            if (immediate)
            {
                canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
                OnHidden();
                return;
            }

            // Play click sound when closing
            if (TK.Audio.AudioManager.Instance != null)
            {
                TK.Audio.AudioManager.Instance.PlayButtonClick();
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(0f, transitionDuration).SetEase(hideEase));

            if (scaleTransition && rectTransform != null)
            {
                seq.Join(rectTransform.DOScale(minScale, transitionDuration).SetEase(hideEase));
            }

            seq.OnComplete(() =>
            {
                gameObject.SetActive(false);
                OnHidden();
            });

            transitionTween = seq;
        }

        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
    }
}
