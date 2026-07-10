using UnityEngine;

namespace TK.Module
{
    /// <summary>
    /// Auto-plays an Image1DSpritesheet at a given FPS.
    /// Attach alongside Image1DSpritesheet to make a looping UI image animation.
    /// </summary>
    [RequireComponent(typeof(Image1DSpritesheet))]
    public class Image1DSpritesheetPlayer : MonoBehaviour
    {
        [Header("Playback Settings")]
        [Tooltip("Frames per second for the animation.")]
        [SerializeField] private float fps = 12f;

        [Tooltip("If true, the animation will loop. If false, it stops on the last frame.")]
        [SerializeField] private bool loop = true;

        [Tooltip("If true, the animation starts playing automatically on enable.")]
        [SerializeField] private bool playOnEnable = true;

        private Image1DSpritesheet _spritesheet;
        private float _timer;
        private int _currentFrame;
        private bool _isPlaying;

        private void Awake()
        {
            _spritesheet = GetComponent<Image1DSpritesheet>();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            Stop();
        }

        private void Update()
        {
            if (!_isPlaying || fps <= 0f) return;

            _timer += Time.deltaTime;

            float frameDuration = 1f / fps;
            if (_timer >= frameDuration)
            {
                _timer -= frameDuration;
                AdvanceFrame();
            }
        }

        public void Play()
        {
            _currentFrame = 0;
            _timer = 0f;
            _isPlaying = true;
            _spritesheet.SetFrame(0);
        }

        public void Stop()
        {
            _isPlaying = false;
        }

        private void AdvanceFrame()
        {
            _currentFrame++;

            if (_currentFrame >= _spritesheet.totalFrames)
            {
                if (loop)
                {
                    _currentFrame = 0;
                }
                else
                {
                    _currentFrame = _spritesheet.totalFrames - 1;
                    _isPlaying = false;
                }
            }

            _spritesheet.SetFrame(_currentFrame);
        }
    }
}
