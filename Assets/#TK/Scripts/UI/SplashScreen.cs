using UnityEngine;
using UnityEngine.SceneManagement;
using TK.Data;
using DG.Tweening;

namespace TK.Gameplay
{
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField] private float _splashDuration = 3f;
        private void Start()
        {
            ShowSplashScreen();
        }

        private void ShowSplashScreen()
        {
            DOTween.Sequence()
                .AppendInterval(_splashDuration) 
                .AppendCallback(() => LoadScene());
        }
        private void LoadScene()
        {
            if (!FTUESaveSystem.LoadFTUEFirstLaunchCompleted())
            {
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}

