using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace TK.Gameplay
{
    using Data;
    using Audio;
    using UnityEngine.XR;

    public class LevelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SequenceController _sequenceController;
        [SerializeField] private HandMovement _hand;
        [SerializeField] private ItemSpawner _itemSpawner;
        [SerializeField] private RecipeData[] _arrRecipes;
        [SerializeField] private GameObject _objRaccoon;

        [Header("Intro Timing")]
        [SerializeField] private float fadeDuration = 0.18f;

        [Header("End Sequence")]
        [SerializeField] private ResultController _result;

        public HandMovement Hand => _hand;
        public static LevelManager Instance { get; protected set; }
        public PlayerMovement GetPlayerMovement => _player;
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
            _objRaccoon.SetActive(false);

            StartCoroutine(FadeGameplayVisuals());

            _hand.ChangeStateToDescent();      
            AudioManager.Instance.PlayGameplayMusic();
            IsGameStarted = true;
        }

        IEnumerator FadeGameplayVisuals()
        {
            float time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float t = time / fadeDuration;

                // SetHandAlpha(t);
                // SetArmAlpha(t);

                yield return null;
            }

            // SetHandAlpha(1f);
            // SetArmAlpha(1f);
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