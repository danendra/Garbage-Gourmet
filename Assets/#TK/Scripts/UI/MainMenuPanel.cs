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

        protected CanvasGroup canvasGroup;
        protected RectTransform rectTransform;
        protected Tween transitionTween;

        public bool IsShown { get; private set; }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        protected virtual void Awake()
        {
            EnsureCanvasGroup();
            rectTransform = GetComponent<RectTransform>();
        }

        public virtual void Show(bool immediate = false)
        {
            IsShown = true;
            gameObject.SetActive(true);

            EnsureCanvasGroup();

            if (transitionTween != null) transitionTween.Kill();

            if (immediate)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                OnShown();
                return;
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            if (TK.Audio.AudioManager.Instance != null)
            {
                TK.Audio.AudioManager.Instance.PlayPanelOpen();
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(1f, transitionDuration).From(0f));

            seq.OnComplete(() =>
            {
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.interactable = true;
                }
                OnShown();
            });

            transitionTween = seq;
        }

        public virtual void Hide(bool immediate = false)
        {
            IsShown = false;

            EnsureCanvasGroup();

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            if (transitionTween != null) transitionTween.Kill();

            if (MainMenuNavigationManager.Instance != null && MainMenuNavigationManager.Instance.ActivePanel == this)
            {
                MainMenuNavigationManager.Instance.NotifyPanelClosed(this);
            }

            if (immediate)
            {
                canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
                OnHidden();
                return;
            }

            if (TK.Audio.AudioManager.Instance != null)
            {
                TK.Audio.AudioManager.Instance.PlayPanelClose();
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(0f, transitionDuration).SetEase(hideEase));

            seq.OnComplete(() =>
            {
                gameObject.SetActive(false);
                OnHidden();
            });

            transitionTween = seq;
        }

        public virtual bool ShowXP => false;

        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
    }
}
