using TK.Gameplay;
using UnityEngine;
using TMPro;

namespace TK.UI
{
    using Gameplay;
    public class ArmIndicator : MonoBehaviour
    {
        [SerializeField] private HandMovement _handMovement;
        [SerializeField] private TextMeshProUGUI _armIndicatorText;

        private void Update()
        {
            _armIndicatorText.text = $"{Mathf.RoundToInt(_handMovement.GetDepth())}M";
        }
    }
}
