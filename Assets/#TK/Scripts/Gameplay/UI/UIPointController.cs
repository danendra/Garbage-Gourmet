using UnityEngine;
using TMPro;

using Anoa;

namespace TK.UI
{
    using Gameplay;

    public class UIPointController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _txtPoint;

        private float _floatCurrentPoint;
        private float _floatTargetPoint;
        private float _fltSpeed;

        private bool _isUpdated;        

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _floatCurrentPoint = GameManager.Instance.intCurrentMoney;
            _txtPoint.text = AnoaModule.ConvertCurency(_floatCurrentPoint);

            _isUpdated = false;

            GameManager.Instance.OnPointUpdate += UpdatePoint;
        }

        void OnDestroy()
        {
            GameManager.Instance.OnPointUpdate -= UpdatePoint;
        }

        public void UpdatePoint()
        {
            _floatTargetPoint = GameManager.Instance.intCurrentMoney;

            _fltSpeed = _floatTargetPoint - _floatCurrentPoint;

            _isUpdated = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (_isUpdated)
            {
                _floatCurrentPoint = Mathf.MoveTowards(_floatCurrentPoint, _floatTargetPoint, Time.deltaTime * _fltSpeed);

                _txtPoint.text = AnoaModule.ConvertCurency(_floatCurrentPoint);

                if (_floatCurrentPoint >= _floatTargetPoint)
                {
                    _isUpdated = false;
                }
            }
        }
    }
}