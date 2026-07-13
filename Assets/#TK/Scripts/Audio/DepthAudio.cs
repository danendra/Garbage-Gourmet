using TK.Gameplay;
using UnityEngine;
using UnityEngine.Audio;

namespace TK.Audio
{
    public class DepthAudio : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HandMovement _playerHand;

        [Header("Threshold")]
        [SerializeField] private float _minimumDepthThreshold = 5f;

        [Header("Audio Settings")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private AudioMixerGroup _outputMixerGroup;

        [Range(0f, 3f)]
        [SerializeField] private float _minVolume = 0f;
        [Range(0f, 3f)]
        [SerializeField] private float _maxVolume = 1f;

        [SerializeField] private float _minPitch = 0.5f;
        [SerializeField] private float _maxPitch = 2f;

        private void Start()
        {
            if (_outputMixerGroup != null)
                _audioSource.outputAudioMixerGroup = _outputMixerGroup;

            _audioSource.clip = _audioClip;
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;

             _audioSource.volume = 0f;
            _audioSource.pitch = _minPitch;
        }

        private void Update()
        {
           float depth = _playerHand.GetDepth();

            if (depth < _minimumDepthThreshold)
            {
               if (_audioSource.isPlaying)
                    _audioSource.Stop();

                return;
            }

            float range = Mathf.Max(_playerHand.GetMaxReach - _minimumDepthThreshold, 0.0001f);
            float t = Mathf.Clamp01((depth - _minimumDepthThreshold) / range);

            _audioSource.volume = Mathf.Lerp(_minVolume, _maxVolume, t);
            _audioSource.pitch  = Mathf.Lerp(_minPitch,  _maxPitch,  t);

            if (!_audioSource.isPlaying)
                _audioSource.Play();
        }
    }
}
