using UnityEngine;
using UnityEngine.UI;

namespace TK.UI
{
    public class InventoryBoxController : MonoBehaviour
    {
        [SerializeField] private Image _imgBg;
        [SerializeField] private Image _imgIcon;
        [SerializeField] private Sprite _sprEmptyBg;
        [SerializeField] private Sprite _sprFillBg;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void AddItem(Sprite _sprite)
        {
            _imgBg.sprite = _sprFillBg;
            _imgIcon.sprite = _sprite;

            _imgIcon.gameObject.SetActive(true);
        }

        public void RemoveItem()
        {
            _imgBg.sprite = _sprEmptyBg;

            _imgIcon.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}