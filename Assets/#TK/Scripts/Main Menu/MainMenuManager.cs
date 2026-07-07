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
        public static MainMenuManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private RectTransform tapToStart;
        [SerializeField] private RectTransform settingsButton;
        [SerializeField] private RectTransform collectionButton;
        [SerializeField] private RectTransform upgradeButton;

        [Header("Animations")]
        [SerializeField] private DOTweenAnimation hatchOpenAnim;

        [Header("Animation Settings")]
        [SerializeField] private string animIdIntro = "intro";
        [SerializeField] private string animIdIdle = "idle";
        [SerializeField] private string animIdExit = "exit";

        [Header("Timing Fallbacks & Delays")]
        [SerializeField] private float hatchFallbackDuration = 1.8f;
        [SerializeField] private float noHandExitDuration = 0.25f;
        [SerializeField] private float introTransitionDelay = 0.5f;

        private UnityEngine.UI.Button playButtonComponent;
        private bool readyToStart;
        private bool starting;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (tapToStart != null)
            {
                playButtonComponent = tapToStart.GetComponent<UnityEngine.UI.Button>();
                if (playButtonComponent == null) playButtonComponent = tapToStart.GetComponentInParent<UnityEngine.UI.Button>();
                if (playButtonComponent == null) playButtonComponent = tapToStart.GetComponentInChildren<UnityEngine.UI.Button>();
            }

            if (playButtonComponent != null)
            {
                playButtonComponent.onClick.AddListener(OnPlayButtonClicked);

                TMPro.TextMeshProUGUI txtComp = playButtonComponent.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (txtComp != null)
                {
                    txtComp.raycastTarget = false;
                }

                tapToStart = playButtonComponent.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogWarning("[MainMenuManager] No Button component found on or around tapToStart! Please assign the PlayButton to tapToStart in the Inspector.");
            }
        }

        IEnumerator Start()
        {
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

            PlayAllById(transform, animIdIntro);

            yield return new WaitForSeconds(introTransitionDelay);
            StartIdleAndReady();
        }

        public void StartIdleAndReady()
        {
            if (readyToStart) return;
            PlayAllById(transform, animIdIdle);
            readyToStart = true;
            AudioManager.Instance.PlayMenuMusic();
        }

        public void PlayHatchSFX()
        {
            AudioManager.Instance.PlayButtonClick();
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

            KillAllById(transform, animIdIntro);
            KillAllById(transform, animIdIdle);
            if (hatchOpenAnim != null) hatchOpenAnim.DOKill();

            SetMenuButtonsState(false);

            RecreateTweenAndPlayAllById(transform, animIdExit);
            AudioManager.Instance.PlayButtonClick();
            yield return new WaitForSeconds(noHandExitDuration);

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

        public void SetMenuButtonsState(bool visible)
        {
            if (playButtonComponent != null)
            {
                var img = playButtonComponent.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.raycastTarget = visible;
                }
            }

            // Get the root GameObject of the play button to prevent deactivating it under any circumstance
            GameObject playButtonGO = null;
            if (playButtonComponent != null) playButtonGO = playButtonComponent.gameObject;
            else if (tapToStart != null) playButtonGO = tapToStart.gameObject;

            // Safety check: Do not deactivate the play button if it's assigned to any other slot by mistake
            if (upgradeButton != null && upgradeButton.gameObject != playButtonGO && (tapToStart == null || upgradeButton.gameObject != tapToStart.gameObject)) 
            {
                upgradeButton.gameObject.SetActive(visible);
            }

            if (settingsButton != null && settingsButton.gameObject != playButtonGO && (tapToStart == null || settingsButton.gameObject != tapToStart.gameObject)) 
            {
                settingsButton.gameObject.SetActive(visible);
            }

            if (collectionButton != null && collectionButton.gameObject != playButtonGO && (tapToStart == null || collectionButton.gameObject != tapToStart.gameObject)) 
            {
                collectionButton.gameObject.SetActive(visible);
            }
        }
    }
}