using UnityEngine;
using UnityEngine.UI;

namespace TK.UI
{
    using Gameplay;
    public class ArmIndicator : MonoBehaviour
    {
        [SerializeField] private HandMovement _handMovement;
        [SerializeField] private Slider _slider;
        [SerializeField] private RectTransform _rectFlag;

        void Start()
        {
            float _fltPos = Mathf.Lerp(_slider.minValue, _slider.maxValue, _handMovement.GetMaxReach);
            Vector2 _pos = _rectFlag.anchoredPosition;
            _pos.y = _fltPos;

            _rectFlag.anchoredPosition = _pos;
            Debug.Log(_pos.y);
        }

        private void Update()
        {
            _slider.value = _handMovement.GetDepth();
        }
    }
}
