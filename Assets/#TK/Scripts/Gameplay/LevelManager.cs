using UnityEngine;
using System.Collections;

namespace TK.Gameplay
{
    using Module;

    public class LevelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement player;
        [SerializeField] private CameraMovement cam;
        [SerializeField] private TransitionAnimationController startController;

        [Header("Gameplay Visuals")]
        [SerializeField] private SpriteRenderer handSprite;
        [SerializeField] private LineRenderer armLine;
        // [SerializeField] private ArmLineRenderer arm;

        [Header("Intro Timing")]
        [SerializeField] private float fadeDuration = 0.18f;

        [Header("End Sequence")]
        [SerializeField] private EndSequenceController endSequence;

        public bool gameStarted { get; private set; }
        public bool isGameOver { get; private set; }

        private int finalScore;

        void Start()
        {
            StartCoroutine(BeginIntroSequence());
        }

        IEnumerator BeginIntroSequence()
        {
            gameStarted = false;
            isGameOver = false;

            player.canMove = false;

            // Hide visuals first
            SetHandAlpha(0f);
            SetArmAlpha(0f);

            // CAMERA PAN UP
            yield return cam.PlayIntroPan();

            // RACCOON ANIMATION
            if (startController != null)
                yield return startController.PlaySequence("Play", false);

            // CAMERA DIVE DOWN
            yield return cam.PlayDiveDown();

            // PRELOAD ARM BEFORE GAME START
            // arm.ForceRefresh();

            cam.SnapToPlayer();
            cam.EnableFollow();

            StartCoroutine(FadeGameplayVisuals());

            player.canMove = true;
            AudioManager.Instance.PlayGameplayMusic();
            gameStarted = true;
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
            if (isGameOver) return;
            isGameOver = true;

            finalScore = Mathf.RoundToInt(playerRef.GetDepth() * 10f);
            GameSession.FinalScore = finalScore;
            GameSession.PlayerWon = true;

            playerRef.canMove = false;
            StartCoroutine(RunEndSequence(playerRef, won: true));
        }

        public void LoseGame(PlayerMovement playerRef)
        {
            if (isGameOver) return;
            isGameOver = true;

            GameSession.FinalScore = 0;
            GameSession.PlayerWon = false;

            playerRef.canMove = false;
            StartCoroutine(RunEndSequence(playerRef, won: false));
        }

        private IEnumerator RunEndSequence(PlayerMovement playerRef, bool won)
        {
            yield return StartCoroutine(FadeOutGameplayVisuals());
            yield return StartCoroutine(endSequence.PlayEndSequence(won));
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