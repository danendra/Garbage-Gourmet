using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TK.Audio;
using DG.Tweening;

namespace TK.MainMenu
{
    using Module;

    public class MainMenuManager : MonoBehaviour
    {
        [Header("Transitions")]
        [SerializeField] private TransitionAnimationController introController;
        [SerializeField] private TransitionAnimationController startController;

        [Header("UI")]
        [SerializeField] private GameObject titleGroup;
        [SerializeField] private RectTransform gameTitle;
        [SerializeField] private RectTransform tapToStart;

        [Header("Animations")]
        [SerializeField] private DOTweenAnimation hatchOpenAnim;
        [SerializeField] private DOTweenAnimation titleExitFadeAnim;
        [SerializeField] private DOTweenAnimation handMoveInAnim;
        [SerializeField] private DOTweenAnimation handMoveOutAnim;

        [Header("Raccoon Hand")]
        [SerializeField] private RectTransform raccoonHand;
        [SerializeField] private UnityEngine.UI.Image raccoonHandImage;
        [SerializeField] private RectTransform raccoonArm;
        [SerializeField] private RectTransform tapRipple;

        [Header("Sprites")]
        [SerializeField] private Sprite handOpen;
        [SerializeField] private Sprite handGrab;

        private CanvasGroup titleCanvasGroup;
        private bool readyToStart;
        private bool starting;

        private float handHeight = 420f;
        private float armWidth = 60f;
        private float currentHandStartY;

        private float hatchFallbackDuration = 1.8f;
        private float handMoveInFallbackDuration = 0.22f;
        private float grabToExitDelay = 0.12f;
        private float handMoveOutFallbackDuration = 0.25f;
        private float noHandExitDuration = 0.25f;

        private const string AnimIdIntro = "intro";
        private const string AnimIdIdle = "idle";
        private const string AnimIdExit = "exit";
        private const string AnimIdRipple = "ripple";

        void Awake()
        {
            titleCanvasGroup = titleGroup.GetComponent<CanvasGroup>();
            if (titleCanvasGroup == null)
            {
                titleCanvasGroup = titleGroup.AddComponent<CanvasGroup>();
            }

            if (introController != null)
            {
                UnityEngine.UI.Image transitionImg = introController.GetComponent<UnityEngine.UI.Image>();
                if (transitionImg != null)
                {
                    transitionImg.raycastTarget = false;
                }
            }
            if (startController != null && startController != introController)
            {
                UnityEngine.UI.Image transitionImg = startController.GetComponent<UnityEngine.UI.Image>();
                if (transitionImg != null)
                {
                    transitionImg.raycastTarget = false;
                }
            }

            UnityEngine.UI.Button playButton = null;
            if (tapToStart != null)
            {
                playButton = tapToStart.GetComponent<UnityEngine.UI.Button>();
                if (playButton == null) playButton = tapToStart.GetComponentInParent<UnityEngine.UI.Button>();
                if (playButton == null) playButton = tapToStart.GetComponentInChildren<UnityEngine.UI.Button>();
            }

            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayButtonClicked);

                TMPro.TextMeshProUGUI txtComp = playButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (txtComp != null)
                {
                    txtComp.raycastTarget = false;
                }

                tapToStart = playButton.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogWarning("[MainMenuManager] No Button component found on or around tapToStart! Please assign the PlayButton to tapToStart in the Inspector.");
            }

            if (raccoonHand != null)
            {
                handHeight = raccoonHand.rect.height;
                raccoonHand.gameObject.SetActive(false);
            }
            if (raccoonArm != null)
            {
                armWidth = raccoonArm.rect.width;
            }
            if (tapRipple != null)
            {
                tapRipple.gameObject.SetActive(false);
            }
        }

        private void UpdateArmLength(float startY)
        {
            if (raccoonHand == null || raccoonArm == null) return;

            Vector2 wristPos = raccoonHand.anchoredPosition + (Vector2)(raccoonHand.localRotation * new Vector3(0f, handHeight / 2f, 0f));
            float dist = wristPos.y - startY;

            raccoonArm.sizeDelta = new Vector2(armWidth, dist + 200f);
        }

        public void OnHandMoveUpdate()
        {
            UpdateArmLength(currentHandStartY);
        }

        IEnumerator Start()
        {
            if (titleCanvasGroup != null) titleCanvasGroup.alpha = 0f;
            if (gameTitle != null) gameTitle.localScale = Vector3.zero;
            if (tapToStart != null) tapToStart.localScale = Vector3.zero;
            titleGroup.SetActive(true);

            yield return introController.PlaySequence("Play", false);

            if (hatchOpenAnim != null)
            {
                hatchOpenAnim.RecreateTweenAndPlay();
                if (hatchOpenAnim.tween != null)
                {
                    yield return hatchOpenAnim.tween.WaitForCompletion();
                }
                else
                {
                    yield return new WaitForSeconds(hatchFallbackDuration);
                }
            }

            PlayAllById(titleGroup.transform, AnimIdIntro);
        }

        public void StartIdleAndReady()
        {
            PlayAllById(titleGroup.transform, AnimIdIdle);
            readyToStart = true;
            AudioManager.Instance.PlayMenuMusic();
        }

        public void PlayHatchSFX()
        {
            AudioManager.Instance.PlayButtonClick();
        }

        public void SetHandOpenSprite()
        {
            if (raccoonHandImage != null) raccoonHandImage.sprite = handOpen;
        }

        private void OnPlayButtonClicked()
        {
            if (!readyToStart || starting)
                return;

            StartCoroutine(IEBeginGame());
        }

        IEnumerator IEBeginGame()
        {
            starting = true;

            if (startController != null)
            {
                UnityEngine.UI.Image transitionImg = startController.GetComponent<UnityEngine.UI.Image>();
                if (transitionImg != null)
                {
                    transitionImg.raycastTarget = true;
                }
            }

            KillAllById(titleGroup.transform, AnimIdIntro);
            KillAllById(titleGroup.transform, AnimIdIdle);
            if (hatchOpenAnim != null) hatchOpenAnim.DOKill();

            RectTransform canvasRect = titleGroup.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            Vector2 targetLocalPos = Vector2.zero;

            RectTransform targetButton = tapToStart;
            if (tapToStart != null && tapToStart.GetComponent<UnityEngine.UI.Button>() == null)
            {
                if (titleGroup != null)
                {
                    UnityEngine.UI.Button btn = titleGroup.GetComponentInChildren<UnityEngine.UI.Button>(true);
                    if (btn != null)
                    {
                        targetButton = btn.GetComponent<RectTransform>();
                        Debug.LogWarning($"[MainMenuManager] tapToStart ('{tapToStart.name}') is not a Button. Auto-detected play button: '{targetButton.name}' instead.", this);
                    }
                }
            }

            if (targetButton != null && canvasRect != null && raccoonHand != null && raccoonHandImage != null && handMoveInAnim != null && handMoveOutAnim != null)
            {
                raccoonHand.gameObject.SetActive(true);
                handMoveInAnim.DOKill();
                handMoveOutAnim.DOKill();
                raccoonHand.DOKill();
                Canvas.ForceUpdateCanvases();

                Vector2 localPosRelToCanvas = canvasRect.InverseTransformPoint(targetButton.position);
                Vector2 anchorOffset = (Vector2)raccoonHand.localPosition - raccoonHand.anchoredPosition;
                targetLocalPos = localPosRelToCanvas - anchorOffset;

                targetButton.gameObject.SetActive(false);

                float startY = -canvasRect.rect.height / 2f - 300f;
                currentHandStartY = startY;

                raccoonHand.anchoredPosition = new Vector2(targetLocalPos.x, startY);
                raccoonHand.localScale = Vector3.one;

                float tilt = 3f;
                float rotZ = 180f + tilt;
                raccoonHand.localRotation = Quaternion.Euler(0f, 0f, rotZ);

                Vector2 dir = (Vector2)(Quaternion.Euler(0f, 0f, tilt) * Vector2.up);
                Vector2 handTargetPos = targetLocalPos - dir * (handHeight / 2f);

                UpdateArmLength(startY);

                handMoveInAnim.endValueV3 = handTargetPos;
                handMoveInAnim.RecreateTweenAndPlay();
                if (handMoveInAnim.tween != null)
                {
                    yield return handMoveInAnim.tween.WaitForCompletion();
                }
                else
                {
                    yield return new WaitForSeconds(handMoveInFallbackDuration);
                }

                raccoonHandImage.sprite = handGrab;
                AudioManager.Instance.PlayButtonClick();

                if (titleExitFadeAnim != null) titleExitFadeAnim.RecreateTweenAndPlay();
                RecreateTweenAndPlayAllById(titleGroup.transform, AnimIdExit);

                if (tapRipple != null)
                {
                    tapRipple.anchoredPosition = targetLocalPos;
                    tapRipple.gameObject.SetActive(true);
                    tapRipple.localScale = Vector3.zero;

                    CanvasGroup rippleCG = tapRipple.GetComponent<CanvasGroup>();
                    if (rippleCG != null) rippleCG.alpha = 0.6f;

                    RecreateTweenAndPlayAllById(tapRipple, AnimIdRipple);
                }

                yield return new WaitForSeconds(grabToExitDelay);

                handMoveOutAnim.endValueV3 = new Vector2(handTargetPos.x, startY);
                handMoveOutAnim.RecreateTweenAndPlay();
                if (handMoveOutAnim.tween != null)
                {
                    yield return handMoveOutAnim.tween.WaitForCompletion();
                }
                else
                {
                    yield return new WaitForSeconds(handMoveOutFallbackDuration);
                }

                raccoonHand.gameObject.SetActive(false);
            }
            else
            {
                if (titleExitFadeAnim != null) titleExitFadeAnim.RecreateTweenAndPlay();
                RecreateTweenAndPlayAllById(titleGroup.transform, AnimIdExit);
                AudioManager.Instance.PlayButtonClick();
                yield return new WaitForSeconds(noHandExitDuration);
            }

            titleGroup.SetActive(false);

            yield return startController.PlaySequence("Play", false);

            SceneManager.LoadScene("GameScene");
        }

        private void PlayAllById(Transform root, string id)
        {
            foreach (var anim in root.GetComponentsInChildren<DOTweenAnimation>(true))
                if (anim.id == id) anim.DOPlayForward();
        }

        private void KillAllById(Transform root, string id)
        {
            foreach (var anim in root.GetComponentsInChildren<DOTweenAnimation>(true))
                if (anim.id == id) anim.DOKill();
        }

        private void RecreateTweenAndPlayAllById(Transform root, string id)
        {
            foreach (var anim in root.GetComponentsInChildren<DOTweenAnimation>(true))
                if (anim.id == id) anim.RecreateTweenAndPlay();
        }
    }
}