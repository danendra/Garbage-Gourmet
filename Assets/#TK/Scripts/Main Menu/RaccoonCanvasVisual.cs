using UnityEngine;
using TK.Module;
using TK.Gameplay;

namespace TK.MainMenu
{
    public class RaccoonCanvasVisual : MonoBehaviour
    {
        private Image1DSpritesheet _raccoonSpritesheet;

        void Awake()
        {
            _raccoonSpritesheet = GetComponent<Image1DSpritesheet>();
        }
        void Start()
        {
            int totalPoint = GameManager.Instance.intCummulativePoint;
            int[] thresholds = GameManager.Instance.RaccoonStageThresholds;

            int stage = thresholds.Length; // default: past the last threshold
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (totalPoint < thresholds[i])
                {
                    stage = i;
                    break;
                }
            }

            _raccoonSpritesheet.SetFrame(stage);
        }
    }
}

