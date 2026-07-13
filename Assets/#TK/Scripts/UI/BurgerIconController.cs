using UnityEngine;
using UnityEngine.UI;

namespace TK.UI
{
    public class BurgerIconController : MonoBehaviour
    {
        [SerializeField] private Image[] _arrImage;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void Lock()
        {
            for (int i = 0; i < _arrImage.Length; i++)
            {
                _arrImage[i].color = Color.black;
            }
        }

        public void Unlock()
        {
            for (int i = 0; i < _arrImage.Length; i++)
            {
                _arrImage[i].color = Color.white;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}