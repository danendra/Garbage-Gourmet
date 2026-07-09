using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TK.Module
{
    [RequireComponent(typeof(Image))]
    public class Image1DSpritesheet : MonoBehaviour
    {
        [Header("Spritesheet Source")]
        [SerializeField] private List<Sprite> frames = new List<Sprite>();

        [Header("Current Frame")]
        [SerializeField] private int _frameIndex;

        public int totalFrames => frames.Count;

        private Image _image;

        void Awake()
        {
            _image = GetComponent<Image>();
        }

        void Start()
        {
            Apply();
        }

        public void SetFrame(int index)
        {
            _frameIndex = index;
            ClampFrame();
            Apply();
        }

        public void NextFrame()
        {
            SetFrame(_frameIndex + 1);
        }

        public void PreviousFrame()
        {
            SetFrame(_frameIndex - 1);
        }

        private void Apply()
        {
            if (_image == null || totalFrames <= 0) return;
            _image.sprite = frames[_frameIndex];
        }

        private void ClampFrame()
        {
            _frameIndex = Mathf.Clamp(_frameIndex, 0, Mathf.Max(0, totalFrames - 1));
        }
    }
}