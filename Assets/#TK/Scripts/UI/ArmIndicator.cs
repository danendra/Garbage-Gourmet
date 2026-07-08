using UnityEngine;
using UnityEngine.UI;

namespace TK.UI
{
    using Gameplay;
    public class ArmIndicator : MonoBehaviour
    {
        [SerializeField] private HandMovement _handMovement;
        [SerializeField] private Slider _slider;

        private void Update()
        {
            _slider.value = _handMovement.GetDepth();
        }
    }
}
