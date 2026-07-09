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
                       
        }

        public void UpdateFlag()
        {
            float _fltPos = Mathf.Lerp(780, -675, _handMovement.GetMaxReach / 100.0f);
            Vector2 _pos = _rectFlag.anchoredPosition;
            _pos.y = _fltPos;

            _rectFlag.anchoredPosition = _pos; 
        }

        private void Update()
        {
            _slider.value = _handMovement.GetDepth();
        }
    }
}
