using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TK.Audio;
using TK.Data;

namespace TK.MainMenu
{
    using Anoa;
    using Gameplay;

    public class MainMenuManager : MonoBehaviour
    {
        public static MainMenuManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private RectTransform tapToStart;

        [SerializeField] private TMPro.TextMeshProUGUI txtPoint;
        [SerializeField] private GameObject _splashScreen;


        [Header("Timing Fallbacks & Delays")]
        [SerializeField] private float noHandExitDuration = 0.25f;
        [SerializeField] private float introTransitionDelay = 0.5f;

        [Header("Scene Settings")]
        [SerializeField] private string targetSceneName = "GameScene";

        private UnityEngine.UI.Button playButtonComponent;
        private bool starting;

        private int _currentPoint;

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
            if (GameManager.Instance != null && GameManager.Instance.SplashCompleted)
            {
                HideSplash();
            }

            UpdateCurrencyUI();


            yield return new WaitForSeconds(introTransitionDelay);
            StartIdleAndReady();
        }

        public void UpdateCurrencyUI()
        {
            if (GameManager.Instance != null)
            {
                _currentPoint = GameManager.Instance.intCurrentMoney;
                txtPoint.text = AnoaModule.ConvertCurency(_currentPoint);
            }
        }

        public void StartIdleAndReady()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }

            if (FTUEMenuManager.Instance != null)
            {
                if (!FTUESaveSystem.LoadFTUEUpgradeCompleted())
                {
                    FTUEMenuManager.Instance.TryStartUpgradeFTUE();
                }
                else if(!FTUESaveSystem.LoadFTUECollectionCompleted())
                {
                    FTUEMenuManager.Instance.TryStartCollectionFTUE();
                }
            }
        }

        public void SetPlayButtonState(bool interactive)
        {
            if (playButtonComponent != null) playButtonComponent.interactable = interactive;
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
                SetMenuButtonsState(false);
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
        }

        public void BringCoinUIToFront()
        {
            SetCoinUISorting(true);
        }

        public void SetCoinUISorting(bool front, Transform relativeTo = null)
        {
            if (txtPoint != null)
            {
                Transform coinTransform = (txtPoint.transform.parent != null) ? txtPoint.transform.parent : txtPoint.transform;
                if (front)
                {
                    coinTransform.SetAsLastSibling();
                }
                else
                {
                    if (relativeTo != null)
                    {
                        int index = relativeTo.GetSiblingIndex();
                        coinTransform.SetSiblingIndex(index);
                    }
                    else
                    {
                        Transform componentsTransform = coinTransform.parent;
                        if (componentsTransform != null)
                        {
                            Transform panelsTransform = componentsTransform.Find("Panels");
                            if (panelsTransform != null)
                            {
                                int panelsIndex = panelsTransform.GetSiblingIndex();
                                coinTransform.SetSiblingIndex(panelsIndex);
                            }
                            else
                            {
                                coinTransform.SetAsFirstSibling();
                            }
                        }
                        else
                        {
                            coinTransform.SetAsFirstSibling();
                        }
                    }
                }
            }
        }

        public void HideSplash()
        {
            _splashScreen.SetActive(false);
            GameManager.Instance.SetSplashCompleted(true);
        }
    }
}