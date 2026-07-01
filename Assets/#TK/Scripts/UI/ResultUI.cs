using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

namespace TK.UI
{

    using Gameplay;

    public class ResultUI : MonoBehaviour
    {
        [Header("Main UI")]
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text flavorText;

        [Header("Visuals")]
        [SerializeField] private Image itemImage;

        [Header("Stars")]
        [SerializeField] private Image[] stars; // 5 elements
        [SerializeField] private Sprite starFilled;
        [SerializeField] private Sprite starEmpty;

        [Header("Wooden Stamp")]
        [SerializeField] private RectTransform woodenStamp;
        [SerializeField] private float stampOffscreenY = 800f;
        [SerializeField] private float stampOnscreenY = 0f;
        [SerializeField] private float stampSlideDuration = 0.3f;
        [SerializeField] private float stampHoldDuration = 0.2f;

        [Header("Ink Stamp")]
        [SerializeField] private Image inkStamp;
        [SerializeField] private Animator inkStampAnimator;
        [SerializeField] private string[] rarityTriggers; // Common, Uncommon, Rare, Epic, Legendary
        [SerializeField] private string trashTrigger = "Trash";

        [Header("Buttons")]
        [SerializeField] private GameObject retryButton;
        [SerializeField] private GameObject menuButton;

        public void Show()
        {
            gameObject.SetActive(true);
            AudioManager.Instance.PlayResultMusic();
            StartCoroutine(ShowResultSequence());
        }

        private IEnumerator ShowResultSequence()
        {
            PrepareUI();

            // Show everything instantly
            resultText.text = GetResultHeadline(
                GameSession.PlayerWon,
                GameSession.CollectedItemType,
                GameSession.CollectedRarity
            );

            itemImage.sprite = GameSession.CollectedSprite;
            Color c = itemImage.color;
            c.a = 1f;
            itemImage.color = c;

            itemNameText.text = GameSession.CollectedItemName;

            int starCount = GetStarCount(GameSession.CollectedItemType, GameSession.CollectedRarity);
            SetStars(starCount);

            scoreText.text = "(" + GameSession.FinalScore + "pts)";

            flavorText.text = GetFlavorText(
                GameSession.PlayerWon,
                GameSession.CollectedItemType,
                GameSession.CollectedRarity
            );

            // Small pause before stamp sequence
            yield return new WaitForSeconds(0.3f);

            // Wooden stamp drops in
            yield return StampSequence();

            // Ink stamp reveal
            yield return PlayInkStamp();

            // Buttons appear
            yield return new WaitForSeconds(0.3f);
            retryButton.SetActive(true);
            menuButton.SetActive(true);
        }

        private void PrepareUI()
        {
            retryButton.SetActive(false);
            menuButton.SetActive(false);

            inkStamp.gameObject.SetActive(false);

            Color c = itemImage.color;
            c.a = 0f;
            itemImage.color = c;

            // Reset wooden stamp position
            Vector2 pos = woodenStamp.anchoredPosition;
            pos.y = stampOffscreenY;
            woodenStamp.anchoredPosition = pos;
        }

        // =====================================================
        // STARS
        // =====================================================

        private int GetStarCount(ItemType type, Rarity rarity)
        {
            if (type == ItemType.Trash) return 0;

            switch (rarity)
            {
                case Rarity.Common: return 1;
                case Rarity.Uncommon: return 2;
                case Rarity.Rare: return 3;
                case Rarity.Epic: return 4;
                case Rarity.Legendary: return 5;
                default: return 0;
            }
        }

        private void SetStars(int count)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].sprite = i < count ? starFilled : starEmpty;
            }
        }

        // =====================================================
        // STAMP ANIMATIONS
        // =====================================================

        private IEnumerator StampSequence()
        {
            woodenStamp.localScale = new Vector3(1.4f, 1.4f, 1f);
            Vector2 pos = woodenStamp.anchoredPosition;
            pos.y = stampOffscreenY;
            woodenStamp.anchoredPosition = pos;

            // Drop in while shrinking
            float t = 0f;
            while (t < stampSlideDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / stampSlideDuration);
                pos.y = Mathf.Lerp(stampOffscreenY, stampOnscreenY, p);
                woodenStamp.anchoredPosition = pos;
                woodenStamp.localScale = Vector3.Lerp(
                    new Vector3(2.5f, 2.5f, 1f),
                    new Vector3(0.8f, 0.8f, 1f),
                    p
                );
                yield return null;
            }

            // Hold on paper
            yield return new WaitForSeconds(stampHoldDuration);

            // Lift off while growing
            t = 0f;
            while (t < stampSlideDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / stampSlideDuration);
                pos.y = Mathf.Lerp(stampOnscreenY, stampOffscreenY, p);
                woodenStamp.anchoredPosition = pos;
                woodenStamp.localScale = Vector3.Lerp(
                    new Vector3(0.9f, 0.9f, 1f),
                    new Vector3(1.4f, 1.4f, 1f),
                    p
                );
                yield return null;
            }

            woodenStamp.localScale = Vector3.one;
        }
        private IEnumerator PlayInkStamp()
        {
            inkStamp.gameObject.SetActive(true);

            if (GameSession.CollectedItemType == ItemType.Trash)
                inkStampAnimator.SetTrigger(trashTrigger);
            else
            {
                int index = (int)GameSession.CollectedRarity;
                inkStampAnimator.SetTrigger(rarityTriggers[index]);
            }

            yield return null;
            while (inkStampAnimator.IsInTransition(0))
                yield return null;

            yield return new WaitUntil(() =>
                inkStampAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        }

        // =====================================================
        // HEADLINES & FLAVOR
        // =====================================================

        private string GetResultHeadline(bool won, ItemType type, Rarity rarity)
        {
            if (!won || type == ItemType.Trash)
            {
                return RandomFrom(
                    "Only Trash Today...",
                    "Back Hungry...",
                    "Trash Again?!",
                    "No Snacks This Time",
                    "Empty Pawed...",
                    "Bad Catch"
                );
            }

            switch (rarity)
            {
                case Rarity.Rare:
                    return RandomFrom("Now That's a Find!", "Lucky Snack!", "Tasty Treasure!");
                case Rarity.Epic:
                    return RandomFrom("Feast Material!", "Now We're Eating Well!", "Legendary Appetite!");
                case Rarity.Legendary:
                    return RandomFrom("Royal Snack Secured!", "The Ultimate Bite!", "A Meal for the Ages!", "History Has Been Eaten!");
                default:
                    return RandomFrom("Snack Secured!", "Good Haul!", "Tasty Find!", "Tonight We Feast!", "Treat Retrieved!");
            }
        }

        private string GetFlavorText(bool won, ItemType type, Rarity rarity)
        {
            if (!won || type == ItemType.Trash)
            {
                return RandomFrom(
                    "Technically an object.",
                    "Nobody ordered this.",
                    "You brought back disappointment.",
                    "Still not edible.",
                    "Straight from the bin.",
                    "Better luck next bite."
                );
            }

            switch (rarity)
            {
                case Rarity.Common: return RandomFrom("A humble snack, but appreciated all the same.", "Nothing fancy—still tasty.", "Simple food. Honest flavor.");
                case Rarity.Uncommon: return RandomFrom("Better than average and twice as exciting.", "A respectable snack with promise.", "You've got an eye for quality.");
                case Rarity.Rare: return RandomFrom("Hard to find, easy to love.", "This one was worth the trip.", "A premium discovery.");
                case Rarity.Epic: return RandomFrom("Rich aroma. Elite status.", "The kind of snack others dream about.", "A feast-worthy treasure.");
                case Rarity.Legendary: return RandomFrom("Whispers spoke of this flavor.", "Some thought it was only a myth.", "The snack of legends has returned.");
                default: return "";
            }
        }

        // =====================================================
        // HELPERS
        // =====================================================

        private string RandomFrom(params string[] options)
        {
            return options[Random.Range(0, options.Length)];
        }

        // =====================================================
        // BUTTONS
        // =====================================================

        public void RetryGame()
        {
            AudioManager.Instance.PlayButtonClick();
            SceneManager.LoadScene("GameScene");
        }

        public void MainMenu()
        {
            AudioManager.Instance.PlayButtonClick();
            SceneManager.LoadScene("MainMenu");
        }
    }
}