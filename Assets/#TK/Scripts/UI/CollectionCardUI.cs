using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using TK.Data;
using System.Collections.Generic;

namespace TK.UI
{
    public class CollectionCardUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References (Front)")]
        [SerializeField] private GameObject frontFace;
        [SerializeField] private Transform _transIconParent;
        [SerializeField] private TextMeshProUGUI collectionNameText;
        [SerializeField] private TextMeshProUGUI multiplierText;

        [SerializeField] private GameObject lockedNameScribble;
        [SerializeField] private GameObject lockedMultiplierScribble;

        [Header("UI References (Back)")]
        [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("backFace")] private GameObject backFaceLocked;
        [SerializeField] private GameObject backFaceUnlocked;
        [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("backDescriptionText")] private TextMeshProUGUI lockedBackDescriptionText;
        [SerializeField] private TextMeshProUGUI unlockedBackDescriptionText;
        [SerializeField] private TextMeshProUGUI unlockedBackNameText;

        [Header("Settings")]
        [SerializeField] private float flipDuration = 0.4f;
        [SerializeField] private string multiplierFormat = "X{0} Multiplier";

        private Dictionary<string, BurgerIconController> _dictBurgerIcons = new Dictionary<string, BurgerIconController>();

        private RecipeData currentCollectionData;
        private BurgerIconController _iconBurger;
        private bool isUnlockedStatus = false;
        private bool isFlipped = false;
        private bool isAnimating = false;

        public void Setup(RecipeData collectionData)
        {
            Clear();

            gameObject.SetActive(true);

            if (_dictBurgerIcons.ContainsKey(collectionData.name))
            {
                _iconBurger = _dictBurgerIcons[collectionData.name];
            }
            else
            {
                _iconBurger = Instantiate(collectionData.rectPrefab, _transIconParent.position, Quaternion.identity, _transIconParent).GetComponent<BurgerIconController>();
                _dictBurgerIcons.Add(collectionData.name, _iconBurger);

                RectTransform _rectIcon = _iconBurger.GetComponent<RectTransform>();
                
                _rectIcon.anchoredPosition = Vector2.zero;
            }

            _iconBurger.gameObject.SetActive(true);

            currentCollectionData = collectionData;
            isUnlockedStatus = !collectionData.IsNew();
            isFlipped = false;
            isAnimating = false;

            transform.DOKill();
            transform.localRotation = Quaternion.identity;
            if (frontFace != null) frontFace.SetActive(true);
            if (backFaceLocked != null) backFaceLocked.SetActive(false);
            if (backFaceUnlocked != null) backFaceUnlocked.SetActive(false);

            if (lockedBackDescriptionText != null) lockedBackDescriptionText.text = collectionData.CollectionDescription;
            if (unlockedBackDescriptionText != null) unlockedBackDescriptionText.text = collectionData.CollectionDescription;
            if (unlockedBackNameText != null) unlockedBackNameText.text = !string.IsNullOrEmpty(collectionData.CollectionName) ? collectionData.CollectionName : collectionData.name;

            if (isUnlockedStatus)
            {

                if (lockedNameScribble != null) lockedNameScribble.SetActive(false);
                if (lockedMultiplierScribble != null) lockedMultiplierScribble.SetActive(false);

                if (collectionNameText != null)
                {
                    collectionNameText.gameObject.SetActive(true);
                    collectionNameText.text = !string.IsNullOrEmpty(collectionData.CollectionName) ? collectionData.CollectionName : collectionData.name;
                }

                if (multiplierText != null)
                {
                    multiplierText.gameObject.SetActive(true);
                    multiplierText.text = string.Format(multiplierFormat, collectionData.FltMultiplier);
                }

                _iconBurger.Unlock();
            }
            else
            {

                if (lockedNameScribble != null) lockedNameScribble.SetActive(true);
                if (lockedMultiplierScribble != null) lockedMultiplierScribble.SetActive(true);

                if (collectionNameText != null) collectionNameText.gameObject.SetActive(false);
                if (multiplierText != null) multiplierText.gameObject.SetActive(false);

                _iconBurger.Lock();
            }
        }

        public void Clear()
        {
            currentCollectionData = null;
            gameObject.SetActive(false);

            foreach(KeyValuePair<string, BurgerIconController> _burger in _dictBurgerIcons)
            {
                _burger.Value.gameObject.SetActive(false);
            }

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isAnimating || currentCollectionData == null) return;

            FlipCard();
        }

        private void FlipCard()
        {
            isAnimating = true;
            isFlipped = !isFlipped;

            float halfDuration = flipDuration / 2f;
            transform.DORotate(new Vector3(0, 90, 0), halfDuration).SetEase(Ease.InQuad).OnComplete(() =>
            {
                if (frontFace != null) frontFace.SetActive(!isFlipped);
                
                if (isFlipped)
                {
                    if (isUnlockedStatus)
                    {
                        if (backFaceUnlocked != null)
                        {
                            backFaceUnlocked.SetActive(true);
                            if (backFaceLocked != null) backFaceLocked.SetActive(false);
                        }
                        else
                        {
                            if (backFaceLocked != null) backFaceLocked.SetActive(true);
                        }
                    }
                    else
                    {
                        if (backFaceLocked != null)
                        {
                            backFaceLocked.SetActive(true);
                            if (backFaceUnlocked != null) backFaceUnlocked.SetActive(false);
                        }
                        else
                        {
                            if (backFaceUnlocked != null) backFaceUnlocked.SetActive(true);
                        }
                    }
                }
                else
                {
                    if (backFaceLocked != null) backFaceLocked.SetActive(false);
                    if (backFaceUnlocked != null) backFaceUnlocked.SetActive(false);
                }

                float targetY = isFlipped ? 180f : 0f;
                transform.DORotate(new Vector3(0, targetY, 0), halfDuration).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    isAnimating = false;
                });
            });
        }
    }
}
