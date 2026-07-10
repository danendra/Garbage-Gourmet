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

            if (totalPoint < 100000) _raccoonSpritesheet.SetFrame(0);
            else if (totalPoint < 800000) _raccoonSpritesheet.SetFrame(1);
            else if (totalPoint < 1500000) _raccoonSpritesheet.SetFrame(2);
            else if (totalPoint < 4000000) _raccoonSpritesheet.SetFrame(3);
            else if (totalPoint < 8000000) _raccoonSpritesheet.SetFrame(4);
            else _raccoonSpritesheet.SetFrame(5);
        }
    }
}

