using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TK.Gameplay
{
    public class LightController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Light2D _globalLight;
        [SerializeField] private HandMovement _handMovement;
        [SerializeField] private Light2D _handSpotLight;
        [SerializeField] private Transform _targetPosition;

        [Header("Light Settings")]
        [SerializeField] private float _globalLightTargetIntensity = 0.02f;
        [SerializeField] private float _handSpotLightTargetIntensity = 1f;
        [SerializeField] private float _maxFadeDistance = 10f;
        [SerializeField] private float _lerpSpeed = 5f;
        [SerializeField] private float _startHandSpotLightIntensity = 0f;

        private float _startGlobalLightIntensity = 1f;
        private float _globalLightCurrentIntensity;
        private float _handSpotLightCurrentIntensity;

        private void Awake()
        {
            if (_globalLight == null || _handSpotLight == null || _handMovement == null)
            {
                enabled = false; 
                return;
            }

            _startGlobalLightIntensity = _globalLight.intensity;

            _globalLightCurrentIntensity = _startGlobalLightIntensity;
            _handSpotLightCurrentIntensity = _startHandSpotLightIntensity;
        }

        private void Update()
        {
            if (_targetPosition == null) return;
            float distance = Vector3.Distance(_handMovement.transform.position, _targetPosition.position);
            float distanceRatio = Mathf.Clamp01(distance / _maxFadeDistance);

            float targetGlobalIntensity = Mathf.Lerp(_globalLightTargetIntensity, _startGlobalLightIntensity, distanceRatio);

            float targetHandSpotIntensity = Mathf.Lerp(_handSpotLightTargetIntensity, _startHandSpotLightIntensity, distanceRatio);

            _globalLightCurrentIntensity = Mathf.Lerp(_globalLightCurrentIntensity, targetGlobalIntensity, Time.deltaTime * _lerpSpeed);
            _handSpotLightCurrentIntensity = Mathf.Lerp(_handSpotLightCurrentIntensity, targetHandSpotIntensity, Time.deltaTime * _lerpSpeed);

            _globalLight.intensity = _globalLightCurrentIntensity;
            _handSpotLight.intensity = _handSpotLightCurrentIntensity;
        }
    }
}

