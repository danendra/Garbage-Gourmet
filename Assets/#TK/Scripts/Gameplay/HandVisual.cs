using System;
using UnityEngine;
using DG.Tweening;

namespace TK.Gameplay
{
    public enum HAND_VISUAL_STATE
    {
        Idle,
        Grab
        
    }

    public class HandVisual : MonoBehaviour
    {

        [Header("Components")]
        [SerializeField] private PlayerInventory _inventory;

        public Transform Transform => transform;

        // components
        private Animator _animator;

        private static readonly int IdleTrigger = Animator.StringToHash("Idle");
        private static readonly int GrabTrigger = Animator.StringToHash("Grab");
        private static readonly int QuickGrabTrigger = Animator.StringToHash("QuickGrab");

        // state
        private HAND_VISUAL_STATE _state = HAND_VISUAL_STATE.Idle;
        public HAND_VISUAL_STATE State
        {
            get => _state;
            set
            {
                if (_state == value) return;
                _state = value;

                HandleStateChange();
            }
        }
        public void ChangeStateToIdle() => State = HAND_VISUAL_STATE.Idle;
        public void ChangeStateToGrab() => State = HAND_VISUAL_STATE.Grab;

        // tween
        private Vector3 _baseScale;
        private Sequence _scaleSequence;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            _inventory.OnItemAdded += HandleItemAdded;
        }

        private void OnDisable()
        {
            _inventory.OnItemAdded -= HandleItemAdded;
        }

        private void Start()
        {
            HandleStateChange();
        }

        private void HandleStateChange()
        {
            PlaySquashStretch(intensity: 1.5f, timeMultiplier: 1f);
            switch (_state)
            {
                case HAND_VISUAL_STATE.Idle:
                    _animator.SetTrigger(IdleTrigger);
                    
                    break;
                case HAND_VISUAL_STATE.Grab:
                    _animator.SetTrigger(GrabTrigger);
                    break;
            }
        }

        private void HandleItemAdded(CollectibleController collectible)
        {
            PlaySquashStretch();
            _animator.SetTrigger(QuickGrabTrigger);
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