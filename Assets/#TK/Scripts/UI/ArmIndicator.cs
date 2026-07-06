using TK.Gameplay;
using UnityEngine;
using TMPro;

namespace TK.UI
{
    using Gameplay;
    public class ArmIndicator : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private TextMeshProUGUI _armIndicatorText;

        private void Update()
        {
            _armIndicatorText.text = $"{Mathf.RoundToInt(_playerMovement.DistanceTravelled)}M";
        }
    }
}
