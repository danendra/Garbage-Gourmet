using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

namespace TK.MainMenu
{
    using Module;

    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private TransitionAnimationController introController;
        [SerializeField] private TransitionAnimationController startController;

        [SerializeField] private GameObject titleGroup;

        private bool readyToStart = false;
        private bool starting = false;

        IEnumerator Start()
        {
            titleGroup.SetActive(false);

            // play intro animation            
            yield return introController.PlaySequence("Play", false);            

            // show title
            titleGroup.SetActive(true);

            readyToStart = true;

            AudioManager.Instance.PlayMenuMusic();
        }

        void Update()
        {
            if (!readyToStart || starting)
                return;

            if (WasTapped())
            {
                StartCoroutine(BeginGame());
            }
        }

        IEnumerator BeginGame()
        {
            starting = true;

            titleGroup.SetActive(false);

            yield return startController.PlaySequence("Play", false);

            SceneManager.LoadScene("GameScene");
        }

        bool WasTapped()
        {
            return Touchscreen.current != null &&
                   Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        }
    }
}