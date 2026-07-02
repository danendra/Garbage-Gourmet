using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

namespace TK.MainMenu
{
    using Module;

    public class MainMenuManager : MonoBehaviour
    {
        [Header("Transitions")]
        [SerializeField] private TransitionAnimationController introController;
        [SerializeField] private TransitionAnimationController startController;

        [Header("UI Elements")]
        [SerializeField] private GameObject titleGroup;
        [SerializeField] private RectTransform gameTitle;
        [SerializeField] private RectTransform tapToStart;

        [Header("DOTween Pro Animations - Intro / Idle")]
        [SerializeField] private DOTweenAnimation hatchOpenAnim;
        [SerializeField] private DOTweenAnimation titleFadeIntro;
        [SerializeField] private DOTweenAnimation titleDropIntro;
        [SerializeField] private DOTweenAnimation titlePopIntro;
        [SerializeField] private DOTweenAnimation playButtonIntro;
        [SerializeField] private DOTweenAnimation titleFloatIdle;
        [SerializeField] private DOTweenAnimation titleTiltIdle;
        [SerializeField] private DOTweenAnimation playButtonPulseIdle;

        [Header("DOTween Pro Animations - Exit / Tap Sequence")]
        [SerializeField] private DOTweenAnimation titleExitFadeAnim;
        [SerializeField] private DOTweenAnimation titleExitScaleAnim;
        [SerializeField] private DOTweenAnimation rippleScaleAnim;    
        [SerializeField] private DOTweenAnimation rippleFadeAnim;     
        [SerializeField] private DOTweenAnimation handMoveInAnim;    
        [SerializeField] private DOTweenAnimation handMoveOutAnim;    

        [Header("Raccoon Hand Animation")]
        [SerializeField] private RectTransform raccoonHand;
        [SerializeField] private UnityEngine.UI.Image raccoonHandImage;
        [SerializeField] private RectTransform raccoonArm;
        [SerializeField] private RectTransform tapRipple;

        [Header("Hand Sprites")]
        [SerializeField] private Sprite handOpen;
        [SerializeField] private Sprite handGrab;

        private CanvasGroup titleCanvasGroup;
        private bool readyToStart = false;
        private bool starting = false;

        private float handHeight = 420f;
        private float armWidth = 60f;
        private float currentHandStartY;

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
                AudioManager.Instance.PlayButtonClick();
                hatchOpenAnim.RecreateTweenAndPlay();
                if (hatchOpenAnim.tween != null)
                {
                    yield return hatchOpenAnim.tween.WaitForCompletion();
                }
                else
                {
                    yield return new WaitForSeconds(1.8f);
                }
            }

            if (titleFadeIntro != null) titleFadeIntro.DOPlayForward();
            if (titleDropIntro != null) titleDropIntro.DOPlayForward();
            if (titlePopIntro != null) titlePopIntro.DOPlayForward();
            if (playButtonIntro != null) playButtonIntro.DOPlayForward();

            yield return new WaitForSeconds(0.8f);

            if (titleFloatIdle != null) titleFloatIdle.DOPlay();
            if (titleTiltIdle != null) titleTiltIdle.DOPlay();
            if (playButtonPulseIdle != null) playButtonPulseIdle.DOPlay();

            readyToStart = true;
            AudioManager.Instance.PlayMenuMusic();
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

            if (titleFadeIntro != null) titleFadeIntro.DOKill();
            if (titleDropIntro != null) titleDropIntro.DOKill();
            if (titlePopIntro != null) titlePopIntro.DOKill();
            if (titleFloatIdle != null) titleFloatIdle.DOKill();
            if (titleTiltIdle != null) titleTiltIdle.DOKill();
            if (playButtonIntro != null) playButtonIntro.DOKill();
            if (playButtonPulseIdle != null) playButtonPulseIdle.DOKill();
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
                raccoonHandImage.sprite = handOpen;
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
                    yield return new WaitForSeconds(0.22f);
                }

                raccoonHandImage.sprite = handGrab;
                AudioManager.Instance.PlayButtonClick();

                if (titleExitFadeAnim != null) titleExitFadeAnim.RecreateTweenAndPlay();
                if (titleExitScaleAnim != null) titleExitScaleAnim.RecreateTweenAndPlay();

                if (tapRipple != null && rippleScaleAnim != null && rippleFadeAnim != null)
                {
                    tapRipple.anchoredPosition = targetLocalPos;
                    tapRipple.gameObject.SetActive(true);
                    tapRipple.localScale = Vector3.zero;

                    CanvasGroup rippleCG = tapRipple.GetComponent<CanvasGroup>();
                    if (rippleCG != null) rippleCG.alpha = 0.6f;

                    rippleScaleAnim.RecreateTweenAndPlay();
                    rippleFadeAnim.RecreateTweenAndPlay(); 
                }

                yield return new WaitForSeconds(0.12f);

                handMoveOutAnim.endValueV3 = new Vector2(handTargetPos.x, startY);
                handMoveOutAnim.RecreateTweenAndPlay();
                if (handMoveOutAnim.tween != null)
                {
                    yield return handMoveOutAnim.tween.WaitForCompletion();
                }
                else
                {
                    yield return new WaitForSeconds(0.25f);
                }

                raccoonHand.gameObject.SetActive(false);
            }
            else
            {
                if (titleExitFadeAnim != null) titleExitFadeAnim.RecreateTweenAndPlay();
                if (titleExitScaleAnim != null) titleExitScaleAnim.RecreateTweenAndPlay();
                AudioManager.Instance.PlayButtonClick();
                yield return new WaitForSeconds(0.25f);
            }

            titleGroup.SetActive(false);

            yield return startController.PlaySequence("Play", false);

            SceneManager.LoadScene("GameScene");
        }
    }
}