using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace TK.Gameplay
{
    using Data;
    using Audio;
    using UnityEngine.XR;
    using TK.UI;

    public class LevelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SequenceController _sequenceController;
        [SerializeField] private HandMovement _hand;
        [SerializeField] private ItemSpawner _itemSpawner;
        [SerializeField] private RecipeData[] _arrRecipes;
        [SerializeField] private RaccoonVisual _raccoonVisual;

        [Header("Intro Timing")]
        [SerializeField] private float fadeDuration = 0.18f;

        [Header("End Sequence")]
        [SerializeField] private ResultController _result;

        public HandMovement Hand => _hand;
        public static LevelManager Instance { get; protected set; }
        public HandMovement GetHandMovement => _hand;
        public PlayerInventory GetPlayerInventory { get; protected set; }        
        public bool IsGameStarted { get; private set; }
        public bool IsGameOver { get; private set; }

        private int finalScore;

        void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            // StartCoroutine(BeginIntroSequence());

            GetPlayerInventory = _hand.GetComponent<PlayerInventory>();

            StartIntro();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ApplyUpgradesToPlayer(_hand, GetPlayerInventory);
                Debug.Log($"[LevelManager] Applied upgrades to player: ArmLevel={GameManager.Instance.ArmLevel}, PickUpLevel={GameManager.Instance.PickUpLevel}, ReleaseLevel={GameManager.Instance.ReleaseLevel}");
            }
        }

        void Update()
        {
            if (!IsGameStarted || IsGameOver) return;
            // Ini buat handle dynamic food spawn
            _itemSpawner.TickSpawn(_hand.transform.position.y);
        }

        private void StartIntro()
        {
            IsGameStarted = false;
            IsGameOver = false;

            _hand.ChangeStateToHidden();

            // SetHandAlpha(0f);
            // SetArmAlpha(0f);

            _sequenceController.PlayIntroScene(StartGame);
        }

        private void StartGame()
        {
            // arm.ForceRefresh();
            // _objRaccoon.SetActive(false);
            _raccoonVisual.ChangeStateToIdle();

            UIManager.Instance.ShowGameplay();
            
            _hand.ChangeStateToDescent();
            AudioManager.Instance.PlayGameplayMusic();
            IsGameStarted = true;

            #if UNITY_EDITOR
            // FTUESaveSystem.ResetAll();
            #endif

            if (!FTUESaveSystem.LoadFTUEGameplayCompleted())
                FTUEManager.Instance.StartGameplayFTUE();
            else if (UpgradeSaveSystem.LoadReleaseLevel() >= 1 && !FTUESaveSystem.LoadFTUEReleaseCompleted())
                FTUEManager.Instance.StartReleaseFTUE();
        }

        // =====================
        // WIN / LOSE
        // =====================

        public bool FindRecipe(IEnumerable<CollectibleController> _ieCollectible, out RecipeData _recipe)
        {                    
            for (int i = 0; i < GameManager.Instance.GetAllRecipes.Length; i++)
            {
                if (GameManager.Instance.GetAllRecipes[i].IsIngredientCorrect(_ieCollectible))
                {
                    _recipe = GameManager.Instance.GetAllRecipes[i];

                    return true;
                }
            }

            _recipe = null;
            return false;
        }

        public void WinGame(HandMovement playerRef)
        {
            if (IsGameOver) return;
            IsGameOver = true;

            finalScore = Mathf.RoundToInt(playerRef.GetDepth() * 10f);
            GameSession.FinalScore = finalScore;
            GameSession.PlayerWon = true;

            StartCoroutine(RunEndSequence(true));
        }

        public void LoseGame(HandMovement playerRef)
        {
            if (IsGameOver) return;
            IsGameOver = true;

            GameSession.FinalScore = 0;
            GameSession.PlayerWon = false;

            StartCoroutine(RunEndSequence(false));
        }

        private IEnumerator RunEndSequence(bool won)
        {
            yield return StartCoroutine(FadeOutGameplayVisuals());

            _sequenceController.PlayEndCamera();

            _result.PlayResult();
        }

        private IEnumerator FadeOutGameplayVisuals()
        {
            float time = fadeDuration;
            while (time > 0f)
            {
                time -= Time.deltaTime;
                float t = time / fadeDuration;
                // SetHandAlpha(t);
                // SetArmAlpha(t);
                yield return null;
            }
            // SetHandAlpha(0f);
            // SetArmAlpha(0f);
        }
    }
}