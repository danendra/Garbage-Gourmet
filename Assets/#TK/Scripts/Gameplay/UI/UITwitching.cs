using UnityEngine;
using DG.Tweening;
using TK.Audio;

public class UITwitching : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private float _twitchDuration = 0.1f;
    [SerializeField] private float _twitchStrength = 10f;
    [SerializeField] private float _twitchDelay = 5f;

    private Ease easeType = Ease.InOutSine;

    public void Twitch()
    {
        float originalZ = _rectTransform.localEulerAngles.z;

        Sequence twitchSequence = DOTween.Sequence();

        Vector3 leftRotation = new Vector3(0, 0, originalZ + _twitchStrength);
        Vector3 centerRotation = new Vector3(0, 0, originalZ);
        Vector3 rightRotation = new Vector3(0, 0, originalZ - _twitchStrength);

        twitchSequence.AppendInterval(_twitchDelay);
        
        for(int i = 0; i < 3; i++)
        {
            if (i==0)
            {
                twitchSequence.AppendCallback(() => AudioManager.Instance.PlaySFX(SFXId.TrashNudge));
            }
            
            twitchSequence.Append(_rectTransform.DOLocalRotate(leftRotation, _twitchDuration).SetEase(easeType));
            twitchSequence.Append(_rectTransform.DOLocalRotate(rightRotation, _twitchDuration).SetEase(easeType));
        }

        twitchSequence.Append(_rectTransform.DOLocalRotate(centerRotation, _twitchDuration).SetEase(easeType));

        twitchSequence.SetLoops(-1, LoopType.Restart);
    }

    private void Start()
    {
        Twitch();
    }
}