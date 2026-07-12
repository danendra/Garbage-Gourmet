using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace TK.UI
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class UIToggleButton : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;

        [Header("Events")]
        public UnityEvent<bool> onToggleChanged;

        private Button _button;
        private Image _image;
        private bool _isOn = true;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();

            _button.onClick.AddListener(OnButtonClicked);
        }

        public void InitializeState(bool startOn)
        {
            _isOn = startOn;
            UpdateVisuals();
        }

        private void OnButtonClicked()
        {
            _isOn = !_isOn;
            UpdateVisuals();
            
            onToggleChanged?.Invoke(_isOn);
        }

        private void UpdateVisuals()
        {
            if (_image != null)
            {
                _image.sprite = _isOn ? onSprite : offSprite;
            }
        }
        
        public bool IsOn => _isOn;
    }
}
