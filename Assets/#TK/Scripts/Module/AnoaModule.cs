using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Anoa
{

    public class AnoaModule : MonoBehaviour
    {

        public void ActiveToogler(GameObject obj)
        {
            obj.SetActive(!obj.activeInHierarchy);
        }

        public static string ConvertCurency(long amount)
        {
            string result = amount.ToString();

            if (amount >= 1000000000000)
            {
                result = ConvertThousand(amount / 1000000000) + "B";
            }
            else if (amount >= 1000000000)
            {
                result = ConvertThousand(amount / 1000000) + "M";
            }
            else if (amount >= 1000000)
            {
                result = ConvertThousand(amount / 1000) + "K";
            }
            //else if (amount >= 1000)
            //{
            //    result = (amount / 1000) + "K";
            //}

            return result;
        }

        public static string ConvertCurency(float amount)
        {
            string result = amount.ToString();

            if (amount >= 1000000000000)
            {
                result = ConvertThousand(amount / 1000000000) + "B";
            }
            else if (amount > 1000000000)
            {
                result = ConvertThousand(amount / 1000000) + "M";
            }
            else if (amount > 1000000)
            {
                result = ConvertThousand(amount / 1000) + "K";
            }
            //else if (amount > 1000)
            //{
            //    result = (amount / 1000).ToString("F2") + "k";
            //}

            return result;
        }

        public static string ConvertThousand(int amount)
        {
            string result = amount.ToString("#,##0");
            return result.Replace(",", ".");
        }

        public static string ConvertThousand(float amount)
        {
            string result = amount.ToString("#,##0");
            return result.Replace(",", ".");
        }

        public static List<T> RandomList<T>(T[] _array)
        {
            List<T> _listT = new List<T>(_array);
            T _temp;
            int _intTemp;

            for (int i = 0; i < _listT.Count; i++)
            {
                _temp = _listT[i];
                _intTemp = Random.Range(0, _listT.Count);
                _listT[i] = _listT[_intTemp];
                _listT[_intTemp] = _temp;
            }

            return _listT;
        }

        public static bool CheckChance(float _fltChance)
        {
            return Random.Range(0.0f, 100.0f) < _fltChance;
        }

        public static void ScrollRectSnapTo(ScrollRect _scroll, RectTransform _rectTarget)
        {
            Canvas.ForceUpdateCanvases();

            RectTransform _rectContent = _scroll.content;

            Vector2 _posSnap = (Vector2)_scroll.transform.InverseTransformPoint(_rectContent.position) / 2
                                - (Vector2)_scroll.transform.InverseTransformPoint(_rectTarget.position);

            // (If your scroll bar is HORIZONTAL, we only change the X axis)
            if (_scroll.horizontal && !_scroll.vertical)
            {
                _rectContent.anchoredPosition = new Vector2(_posSnap.x, _rectContent.anchoredPosition.y);
            }
            // (If your scroll bar is VERTICAL, we only change the Y axis)
            else if (_scroll.vertical && !_scroll.horizontal)
            {
                _rectContent.anchoredPosition = new Vector2(_rectContent.anchoredPosition.x, _posSnap.y);
            }
            // (If it scrolls both ways, apply both)
            else
            {
                _rectContent.anchoredPosition = _posSnap;
            }
        }

        public static float Snap(float _fltValue, float _fltSnapValue)
        {
            return Mathf.Round(_fltValue / _fltSnapValue) * _fltSnapValue;
        }

        public static IEnumerator IEWaitWhileGameObjectActive(GameObject _go)
        {
            yield return new WaitForEndOfFrame();

            while (_go.activeInHierarchy)
            {
                yield return null;
            }
        }
    }
}
