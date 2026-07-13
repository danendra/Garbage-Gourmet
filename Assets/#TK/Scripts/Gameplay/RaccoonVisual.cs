using System;
using UnityEngine;
using DG.Tweening;
using TK.Module;
using TK.Audio;
using System.Collections;

namespace TK.Gameplay
{
    public enum RACCOON_VISUAL_STATE
    {
        Idle,
        Eat,
        Nauseous,
        Trash,
        Explode
        
    }

    public class RaccoonVisual : MonoBehaviour
    {

        [Header("Components")]
        [SerializeField] private GameObject _visualIdle;
        [SerializeField] private GameObject _visualEat;
        [SerializeField] private GameObject _visualNauseous;
        [SerializeField] private GameObject _visualTrash;
        [SerializeField] private GameObject _tail;
        [SerializeField] private GameObject _explosion;
        [SerializeField] private ParticleGroup _foodExplosionParticle;
        
        [Header("Tweening")]
        [SerializeField] private DOTweenAnimation _tweenShake;

        private SpriteRenderer1DSpritesheet _visualIdleSpritesheet;
        private SpriteRenderer1DSpritesheet _visualEatSpritesheet;
        private SpriteRenderer1DSpritesheet _visualNauseousSpritesheet;
        private SpriteRenderer1DSpritesheet _visualTrashSpritesheet;
        private Animator _handAnimator;
        private Animator _explosionAnimator;
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
        public void ChangeStateToNauseous() => State = RACCOON_VISUAL_STATE.Nauseous;
        public void ChangeStateToTrash() => State = RACCOON_VISUAL_STATE.Trash;
        public void ChangeStateToExplode() => State = RACCOON_VISUAL_STATE.Explode;

        // tween
        private Vector3 _baseScale;
        private Sequence _scaleSequence;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _visualIdleSpritesheet = _visualIdle.GetComponent<SpriteRenderer1DSpritesheet>();
            _visualEatSpritesheet = _visualEat.GetComponent<SpriteRenderer1DSpritesheet>();
            _visualEatMaskSpritesheet = _visualEat.GetComponentInChildren<SpriteMask1DSpritesheet>();
            _visualNauseousSpritesheet = _visualNauseous.GetComponent<SpriteRenderer1DSpritesheet>();
            _visualTrashSpritesheet = _visualTrash.GetComponent<SpriteRenderer1DSpritesheet>();
            _handAnimator = _visualTrash.GetComponentInChildren<Animator>();
            _explosionAnimator = _explosion.GetComponent<Animator>();
        }

        private void OnEnable()
        {
            GameManager.Instance.OnPointUpdate += HandlePointUpdate;
            
        }

        private void OnDisable()
        {
            GameManager.Instance.OnPointUpdate -= HandlePointUpdate;
        }


        private void Start()
        {
            HandleStateChange();
            HandlePointUpdate();
            HandleStageChange();
        }

        private void Update()
        {
            // DEBUG
            if (Input.GetKeyDown(KeyCode.Alpha1)) State = RACCOON_VISUAL_STATE.Idle;
            if (Input.GetKeyDown(KeyCode.Alpha2)) State = RACCOON_VISUAL_STATE.Eat;
            if (Input.GetKeyDown(KeyCode.Alpha3)) State = RACCOON_VISUAL_STATE.Nauseous;
            if (Input.GetKeyDown(KeyCode.Alpha4)) State = RACCOON_VISUAL_STATE.Trash;
            if (Input.GetKeyDown(KeyCode.Alpha5)) State = RACCOON_VISUAL_STATE.Explode;
            if (Input.GetKeyDown(KeyCode.Alpha6)) Stage++;
            if (Input.GetKeyDown(KeyCode.Alpha7)) Stage--;
        }

        private void HandleStateChange()
        {
            PlaySquashStretch(intensity: 1.5f, timeMultiplier: 1f);
            _explosion.SetActive(false);  
            _tail.SetActive(true);

            switch (_state)
            {
                case RACCOON_VISUAL_STATE.Idle:
                    _visualIdle.SetActive(true);
                    _visualEat.SetActive(false);
                    _visualNauseous.SetActive(false);
                    _visualTrash.SetActive(false);

                    break;

                case RACCOON_VISUAL_STATE.Eat:
                    _visualIdle.SetActive(false);
                    _visualEat.SetActive(true);
                    _visualNauseous.SetActive(false);
                    _visualTrash.SetActive(false);

                    break;

                case RACCOON_VISUAL_STATE.Nauseous:
                    _visualIdle.SetActive(false);
                    _visualEat.SetActive(false);
                    _visualNauseous.SetActive(true);
                    _visualTrash.SetActive(false);

                    break;
                
                case RACCOON_VISUAL_STATE.Trash:
                    _visualIdle.SetActive(false);
                    _visualEat.SetActive(false);
                    _visualNauseous.SetActive(false);
                    _visualTrash.SetActive(true);               

                    ApplyStageTrigger();
                    break;
                
                case RACCOON_VISUAL_STATE.Explode:
                    _visualIdle.SetActive(false);
                    _visualEat.SetActive(false);
                    _visualNauseous.SetActive(true);
                    _visualTrash.SetActive(false);

                    PlayExplodeExpand();
                                      
                    break;
            }
        }

        private void ApplyStageTrigger()
        {
            if (_handAnimator) _handAnimator.SetTrigger(Animator.StringToHash("Stage" + _stage));
        }

        private void HandlePointUpdate()
        {
            int totalPoint = GameManager.Instance.intCummulativePoint;
            int[] thresholds = GameManager.Instance.RaccoonStageThresholds;

            int stage = thresholds.Length;
            
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (totalPoint < thresholds[i])
                {
                    stage = i;
                    break;
                }
            }

            Stage = stage;
        }

        private void HandleStageChange()
        {
            PlaySquashStretch(intensity: 1.5f, timeMultiplier: 1f);

            if (_visualIdleSpritesheet) _visualIdleSpritesheet.SetFrame(_stage);
            if (_visualEatSpritesheet) _visualEatSpritesheet.SetFrame(_stage);
            if (_visualNauseousSpritesheet) _visualNauseousSpritesheet.SetFrame(_stage);
            if (_visualEatMaskSpritesheet) _visualEatMaskSpritesheet.SetFrame(_stage);

            if (_visualTrashSpritesheet) _visualTrashSpritesheet.SetFrame(_stage);
            ApplyStageTrigger();
        }

        #region  Tweening
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
        
        public void PlayShake()
        {
            _tweenShake.RecreateTweenAndPlay();
        }

        public void PlayExplodeExpand()
        {
            if (_scaleSequence != null && _scaleSequence.IsActive())
                _scaleSequence.Kill();

            _scaleSequence = CreateExplodeExpandTween();
        }

        private Sequence CreateExplodeExpandTween()
        {

            Sequence sequence = DOTween.Sequence();

            sequence.AppendCallback(() => AudioManager.Instance.PlaySFX(SFXId.Expand));
            sequence.Append(transform.DOScale(_baseScale*1.2f, 1/6f).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOScale(_baseScale*1.1f, 1/6f).SetEase(Ease.OutQuad));
            sequence.AppendCallback(() => AudioManager.Instance.PlaySFX(SFXId.Expand));
            sequence.Append(transform.DOScale(_baseScale*1.6f, 1/6f).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOScale(_baseScale*1.4f, 1/6f).SetEase(Ease.OutQuad));
            sequence.AppendCallback(() => AudioManager.Instance.PlaySFX(SFXId.Expand));
            sequence.Append(transform.DOScale(_baseScale*2f, 1/6f).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOScale(_baseScale*1.8f, 1/6f).SetEase(Ease.OutQuad));
            sequence.AppendCallback(() => {
                _explosion.SetActive(true);
                TriggerExplosionAnimation();
                StartCoroutine(TriggerFoodExplosionParticle());
                });
            sequence.Append(transform.DOScale(Vector3.zero, 1/6f).SetEase(Ease.OutQuad));
            sequence.AppendCallback(() => {
                _visualNauseous.SetActive(false);
                _tail.SetActive(false);
                _explosion.SetActive(false);
                });
            // sequence.AppendInterval(4f);

            return sequence;
        }

        private void TriggerExplosionAnimation()
        {
            if (_explosionAnimator) _explosionAnimator.SetTrigger(Animator.StringToHash("Explode"));
        }

        private IEnumerator TriggerFoodExplosionParticle()
        {
            AudioManager.Instance.PlaySFX(SFXId.Explosion);
            _foodExplosionParticle.ChangeOrderInLayer(0);
            _foodExplosionParticle.Play();

            yield return new WaitForSeconds(1f);
            _foodExplosionParticle.ChangeOrderInLayer(200);


        }

        #endregion
    }
}