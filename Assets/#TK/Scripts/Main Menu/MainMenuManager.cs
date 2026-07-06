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

        private CanvasGroup titleCanvasGroup;
        private bool readyToStart;
        private bool starting;

        private float hatchFallbackDuration = 1.8f;
        private float noHandExitDuration = 0.25f;

        private const string AnimIdIntro = "intro";
        private const string AnimIdIdle  = "idle";
        private const string AnimIdExit  = "exit";

        void Awake()
        {
            titleCanvasGroup = titleGroup.GetComponent<CanvasGroup>();
            if (titleCanvasGroup == null)
                titleCanvasGroup = titleGroup.AddComponent<CanvasGroup>();

            if (introController != null)
            {
                UnityEngine.UI.Image img = introController.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.raycastTarget = false;
            }
            if (startController != null && startController != introController)
            {
                UnityEngine.UI.Image img = startController.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.raycastTarget = false;
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
                if (txtComp != null) txtComp.raycastTarget = false;

                tapToStart = playButton.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogWarning("[MainMenuManager] No Button component found on or around tapToStart! Please assign the PlayButton to tapToStart in the Inspector.");
            }
        }

        IEnumerator Start()
        {
            if (titleCanvasGroup != null) titleCanvasGroup.alpha = 0f;
            if (gameTitle != null) gameTitle.localScale = Vector3.zero;
            if (tapToStart != null) tapToStart.localScale = Vector3.zero;
            titleGroup.SetActive(true);

            if (introController != null && introController.gameObject.activeInHierarchy)
                yield return introController.PlaySequence("Play", false);

            if (hatchOpenAnim != null && hatchOpenAnim.enabled)
            {
                hatchOpenAnim.RecreateTweenAndPlay();
                if (hatchOpenAnim.tween != null)
                    yield return hatchOpenAnim.tween.WaitForCompletion();
                else
                    yield return new WaitForSeconds(hatchFallbackDuration);
            }

            PlayAllById(titleGroup.transform, AnimIdIntro);
            StartIdleAndReady();
        }

        public void StartIdleAndReady()
        {
            PlayAllById(titleGroup.transform, AnimIdIdle);

            if (tapToStart != null)
            {
                tapToStart.DOKill();
                tapToStart.localScale = Vector3.one;
            }

            readyToStart = true;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMenuMusic();
        }

        public void PlayHatchSFX()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayButtonClick();
        }

        private void OnPlayButtonClicked()
        {
            if (!readyToStart || starting) return;
            StartCoroutine(IEBeginGame());
        }

        IEnumerator IEBeginGame()
        {
            starting = true;

            if (startController != null)
            {
                UnityEngine.UI.Image img = startController.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.raycastTarget = true;
            }

            KillAllById(titleGroup.transform, AnimIdIntro);
            KillAllById(titleGroup.transform, AnimIdIdle);
            if (hatchOpenAnim != null && hatchOpenAnim.enabled) hatchOpenAnim.DOKill();

            if (titleExitFadeAnim != null && titleExitFadeAnim.enabled) titleExitFadeAnim.RecreateTweenAndPlay();
            RecreateTweenAndPlayAllById(titleGroup.transform, AnimIdExit);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayButtonClick();

            yield return new WaitForSeconds(noHandExitDuration);

            titleGroup.SetActive(false);

            if (startController != null && startController.gameObject.activeInHierarchy)
                yield return startController.PlaySequence("Play", false);

            SceneManager.LoadScene("GameScene");
        }

        private void PlayAllById(Transform root, string id)
        {
            foreach (var anim in root.GetComponentsInChildren<DOTweenAnimation>(true))
                if (anim.id == id && anim.enabled) anim.DOPlayForward();
        }

        private void KillAllById(Transform root, string id)
        {
            foreach (var anim in root.GetComponentsInChildren<DOTweenAnimation>(true))
                if (anim.id == id && anim.enabled) anim.DOKill();
        }

        private void RecreateTweenAndPlayAllById(Transform root, string id)
        {
            foreach (var anim in root.GetComponentsInChildren<DOTweenAnimation>(true))
                if (anim.id == id && anim.enabled) anim.RecreateTweenAndPlay();
        }
    }
}