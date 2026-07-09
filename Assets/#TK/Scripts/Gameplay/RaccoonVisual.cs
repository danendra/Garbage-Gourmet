using System;
using UnityEngine;
using DG.Tweening;
using TK.Module;

namespace TK.Gameplay
{
    public enum RACCOON_VISUAL_STATE
    {
        Idle,
        Eat
        
    }

    public class RaccoonVisual : MonoBehaviour
    {

        [Header("Components")]
        [SerializeField] private GameObject _visualIdle;
        [SerializeField] private GameObject _visualEat;

        private SpriteRenderer1DSpritesheet _visualIdleSpritesheet;
        private SpriteRenderer1DSpritesheet _visualEatSpritesheet;
        private SpriteMask1DSpritesheet _visualEatMaskSpritesheet;

        public Transform Transform => transform;

        // fat stage
        private int _stage = 0;
        public int Stage
        {
            get => _stage;
            set
            {
                if (_stage == value) return;
                _stage = value;
                Debug.Log(_stage);

                HandleStageChange();
            }
        }



        // state
        private RACCOON_VISUAL_STATE _state = RACCOON_VISUAL_STATE.Idle;
        public RACCOON_VISUAL_STATE State
        {
            get => _state;
            set
            {
                if (_state == value) return;
                _state = value;

                HandleStateChange();
            }
        }
        public void ChangeStateToIdle() => State = RACCOON_VISUAL_STATE.Idle;
        public void ChangeStateToEat() => State = RACCOON_VISUAL_STATE.Eat;

        // tween
        private Vector3 _baseScale;
        private Sequence _scaleSequence;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _visualIdleSpritesheet = _visualIdle.GetComponent<SpriteRenderer1DSpritesheet>();
            _visualEatSpritesheet = _visualEat.GetComponent<SpriteRenderer1DSpritesheet>();

            Transform maskChild = _visualEat.transform.Find("Mask");
            _visualEatMaskSpritesheet = maskChild.GetComponent<SpriteMask1DSpritesheet>();
        }

        private void Start()
        {
            HandleStateChange();
            HandleStageChange();
        }

        private void Update()
        {
            // DEBUG
            if (Input.GetKeyDown(KeyCode.Alpha1)) State = RACCOON_VISUAL_STATE.Idle;
            if (Input.GetKeyDown(KeyCode.Alpha2)) State = RACCOON_VISUAL_STATE.Eat;
            if (Input.GetKeyDown(KeyCode.Alpha3)) Stage++;
            if (Input.GetKeyDown(KeyCode.Alpha4)) Stage--;
        }

        private void HandleStateChange()
        {
            PlaySquashStretch(intensity: 1.5f, timeMultiplier: 1f);
            switch (_state)
            {
                case RACCOON_VISUAL_STATE.Idle:
                    _visualIdle.SetActive(true);
                    _visualEat.SetActive(false);
                    break;

                case RACCOON_VISUAL_STATE.Eat:
                    _visualIdle.SetActive(false);
                    _visualEat.SetActive(true);
                    break;
            }
        }

        private void HandleStageChange()
        {
            PlaySquashStretch(intensity: 1.5f, timeMultiplier: 1f);

            if (!_visualIdleSpritesheet) return;
            _visualIdleSpritesheet.SetFrame(_stage);

            if (!_visualEatSpritesheet) return;
            _visualEatSpritesheet.SetFrame(_stage);

            if (!_visualEatMaskSpritesheet) return;
            _visualEatMaskSpritesheet.SetFrame(_stage);
        }

        // squash and stretch
        public void PlaySquashStretch(float intensity = 1f, float timeMultiplier = 1f) 
        {
            if (_scaleSequence != null && _scaleSequence.IsActive())
                _scaleSequence.Kill();

            _scaleSequence = CreateSquashStretchTween(intensity, timeMultiplier);
        }

        private Sequence CreateSquashStretchTween(float intensity = 1f, float timeMultiplier = 1f) 
        {
            Sequence sequence = DOTween.Sequence();

            sequence.Append(transform.DOScale(Squash(intensity), 0.04f * timeMultiplier).SetEase(Ease.InExpo));
            sequence.Append(transform.DOScale(Stretch(intensity), 0.06f * timeMultiplier).SetEase(Ease.OutExpo));
            sequence.Append(transform.DOScale(_baseScale, 0.10f * timeMultiplier).SetEase(Ease.OutExpo));

            return sequence;
        }

        private Vector3 Squash(float intensity) => new Vector3(_baseScale.x * (1f + 0.25f * intensity), _baseScale.y * (1f - 0.30f * intensity), _baseScale.z * (1f + 0.25f * intensity));
        private Vector3 Stretch(float intensity) => new Vector3(_baseScale.x * (1f - 0.15f * intensity), _baseScale.y * (1f + 0.35f * intensity), _baseScale.z * (1f - 0.15f * intensity));
        
    }
}