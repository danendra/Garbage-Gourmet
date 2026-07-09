using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace TK.Gameplay
{
    using Module;

    public class SequenceController : MonoBehaviour
    {
        [SerializeField] private CameraTransition _cameraBottom;
        [SerializeField] private CameraTransition _cameraTop;
        [SerializeField] private CameraTransition _cameraPlayer;
        [SerializeField] private TransitionAnimationController animationRacoon;

        protected CinemachineBrain _cinemachineBrain;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        }

        public void PlayIntroScene(UnityAction _action)
        {
            StartCoroutine(IEPlayIntroScene(_action));
        }

        private IEnumerator IEPlayIntroScene(UnityAction _action)
        {
            yield return new WaitForSeconds(1.0f);

            _cameraPlayer.SetInactive();
            _cameraBottom.SetInactive();
            _cameraTop.SetActive(_cinemachineBrain);

            yield return new WaitForSeconds(_cameraTop.BlendDuration);

            yield return new WaitForSeconds(1.0f);
            ///
            /// IVAN disini animasi korek2 sampah
            //yield return animationRacoon.PlaySequence("Play", false);

            _cameraTop.SetInactive();
            _cameraPlayer.SetActive(_cinemachineBrain);

            yield return new WaitForSeconds(_cameraPlayer.BlendDuration);

            _action.Invoke();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PlayEndCamera()
        {
            _cameraPlayer.SetInactive();
            _cameraBottom.SetInactive();
            _cameraTop.SetActive(_cinemachineBrain, 0.1f);
        }

        [System.Serializable]
        public class CameraTransition
        {
            public GameObject GOCamera;
            public CinemachineBlendDefinition.Styles Styles = CinemachineBlendDefinition.Styles.EaseOut;
            public float BlendDuration = 1.3f;

            public void SetActive(CinemachineBrain _cinemachineBrain)
            {
                _cinemachineBrain.DefaultBlend = new CinemachineBlendDefinition(Styles, BlendDuration);
                GOCamera.SetActive(true);
            }

            public void SetActive(CinemachineBrain _cinemachineBrain, float _fltDuration)
            {
                _cinemachineBrain.DefaultBlend = new CinemachineBlendDefinition(Styles, _fltDuration);
                GOCamera.SetActive(true);
            }

            public void SetInactive()
            {
                GOCamera.SetActive(false);
            }
        }
    }
}