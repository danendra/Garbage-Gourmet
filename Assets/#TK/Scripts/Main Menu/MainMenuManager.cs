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

        [Header("Timing Fallbacks & Delays")]
        [SerializeField] private float hatchFallbackDuration = 1.8f;
        [SerializeField] private float noHandExitDuration = 0.25f;
        [SerializeField] private float introTransitionDelay = 0.5f;

        [Header("Scene Settings")]
        [SerializeField] private string targetSceneName = "GameScene";

        private UnityEngine.UI.Button playButtonComponent;
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
                var hatchImg = hatchOpenAnim.GetComponent<UnityEngine.UI.Image>();
                if (hatchImg != null) hatchImg.raycastTarget = false;

                hatchOpenAnim.RecreateTweenAndPlay();
                yield return new WaitForSeconds(hatchFallbackDuration);
            }

            yield return new WaitForSeconds(introTransitionDelay);
            StartIdleAndReady();
        }

        public void StartIdleAndReady()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }
        }

        public void PlayHatchSFX()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }

        private void OnPlayButtonClicked()
        {
            if (starting)
                return;

            try
            {
                StartCoroutine(IEBeginGame());
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("Error starting IEBeginGame: " + e);
            }
        }

        IEnumerator IEBeginGame()
        {
            starting = true;

            try
            {
                if (hatchOpenAnim != null) hatchOpenAnim.DOKill();

                SetMenuButtonsState(false);

                if (AudioManager.Instance != null) 
                {
                    AudioManager.Instance.PlayButtonClick();
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("Error during IEBeginGame prep: " + e);
            }

            yield return new WaitForSeconds(noHandExitDuration);

            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Error loading {targetSceneName}: " + e);
            }
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

            GameObject playButtonGO = null;
            if (playButtonComponent != null) playButtonGO = playButtonComponent.gameObject;
            else if (tapToStart != null) playButtonGO = tapToStart.gameObject;

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