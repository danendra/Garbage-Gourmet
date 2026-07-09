using UnityEngine;

using DG.Tweening;

namespace TK.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private UIResultController _uiResult;
        [SerializeField] private DOTweenAnimation _tweenStar;
        [SerializeField] private DOTweenAnimation _tweenInventory;
        [SerializeField] private DOTweenAnimation _tweenBar;

        public static UIManager Instance {get; protected set;}
        public UIResultController GetUIResult => _uiResult;

        void Awake()
        {
            Instance = this;
        }

        public void ShowGameplay()
        {
            _tweenInventory.RecreateTweenAndPlay();
            _tweenBar.RecreateTweenAndPlay();
        }

        public void HideGameplay()
        {
            _tweenInventory.transform.DOLocalMoveX(-400, 0.3f).SetEase(Ease.InBack).SetRelative(true);
            _tweenBar.transform.DOLocalMoveX(300, 0.3f).SetEase(Ease.InBack).SetRelative(true);
            _tweenStar.RecreateTweenAndPlay();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}