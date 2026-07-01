using UnityEngine;
using TMPro;

namespace TK.UI
{
    using Gameplay;

    public class GameHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement player;
        [SerializeField] private TextMeshProUGUI depthText;

        void Update()
        {
            UpdateDepthText();
        }

        private void UpdateDepthText()
        {
            int depth = Mathf.RoundToInt(player.GetDepth());
            depthText.text = "Depth: " + depth + "m";
        }
    }
}