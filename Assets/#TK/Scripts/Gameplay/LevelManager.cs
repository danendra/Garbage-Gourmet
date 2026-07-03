using UnityEngine;
using System.Collections;

namespace TK.Gameplay
{
    using Module;

    public class LevelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SequenceController _sequenceController;
        [SerializeField] private PlayerMovement player;                

        [Header("Gameplay Visuals")]
        [SerializeField] private SpriteRenderer handSprite;
        [SerializeField] private LineRenderer armLine;
        [SerializeField] private ArmLineRenderer arm;

        [Header("Intro Timing")]
        [SerializeField] private float fadeDuration = 0.18f;

        [Header("End Sequence")]
        [SerializeField] private ResultController _result;

        public static LevelManager Instance {get; protected set;}
        public bool IsGameStarted { get; private set; }
        public bool IsGameOver { get; private set; }

        private int finalScore;

        void Start()
        {
            // StartCoroutine(BeginIntroSequence());

            StartIntro();
        }

        private void StartIntro()
        {
            IsGameStarted = false;
            IsGameOver = false;

            player._canMove = false;

            SetHandAlpha(0f);
            SetArmAlpha(0f);

            _sequenceController.PlayIntroScene(StartGame);
        }

        private void StartGame()
        {
            arm.ForceRefresh();

            StartCoroutine(FadeGameplayVisuals());

            player._canMove = true;
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

                SetHandAlpha(t);
                SetArmAlpha(t);

                yield return null;
            }

            SetHandAlpha(1f);
            SetArmAlpha(1f);
        }

        void SetHandAlpha(float alpha)
        {
            Color c = handSprite.color;
            c.a = alpha;
            handSprite.color = c;
        }

        void SetArmAlpha(float alpha)
        {
            Color a = armLine.startColor;
            Color b = armLine.endColor;

            a.a = alpha;
            b.a = alpha;

            armLine.startColor = a;
            armLine.endColor = b;
        }

        // =====================
        // WIN / LOSE
        // =====================

        public void WinGame(PlayerMovement playerRef)
        {
            if (IsGameOver) return;
            IsGameOver = true;

            finalScore = Mathf.RoundToInt(playerRef.GetDepth() * 10f);
            GameSession.FinalScore = finalScore;
            GameSession.PlayerWon = true;

            playerRef._canMove = false;
            StartCoroutine(RunEndSequence(true));
        }

        public void LoseGame(PlayerMovement playerRef)
        {
            if (IsGameOver) return;
            IsGameOver = true;

            GameSession.FinalScore = 0;
            GameSession.PlayerWon = false;

            playerRef._canMove = false;
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
                SetHandAlpha(t);
                SetArmAlpha(t);
                yield return null;
            }
            SetHandAlpha(0f);
            SetArmAlpha(0f);
        }
    }
}