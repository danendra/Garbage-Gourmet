using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace TK.MainMenu
{
    public class MainMenuUI : MonoBehaviour
    {
        private bool isStarting = false;

        void Update()
        {
            if (isStarting) return;

            bool tapped = false;

            // Mobile touch
            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                tapped = true;
            }

            // Mouse click for editor testing
            if (Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                tapped = true;
            }

            if (tapped)
            {
                StartGame();
            }
        }

        void StartGame()
        {
            isStarting = true;
            AudioManager.Instance.PlayButtonClick();
            Invoke(nameof(LoadGameScene), 0.15f);
        }

        void LoadGameScene()
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}