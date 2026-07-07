using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

namespace TK.Gameplay
{
    public enum ARM_LINE_STATE
    {
        Inactive,
        Growing,
        Snap,
        Retract
    }

    [RequireComponent(typeof(LineRenderer))]
    public class NewArmLineRenderer : MonoBehaviour
    {
        [SerializeField] private Transform _anchor;
        [SerializeField] private Transform _target;
        [SerializeField] private float _maxDistance = 1f;
        [SerializeField] private float _targetDistance = 0.2f;
        [SerializeField] private float _smoothSpeed = 0.02f;
        [SerializeField] private float _trailSpeed = 360f;

        [SerializeField] private float _wiggleSpeed = 1f;
        [SerializeField] private float _wiggleMagnitude = 5f;

        [SerializeField] private float _snapDuration = 0.4f;

        private Sequence _snapSequence;
        
        private List<Vector3> _pointPositions = new List<Vector3>();
        private List<Vector3> _pointVelocities = new List<Vector3>();

        private int _growIndex;

        // components
        private LineRenderer _lineRenderer;

        // state
        private ARM_LINE_STATE _state = ARM_LINE_STATE.Inactive;
        public ARM_LINE_STATE State
        {
            get => _state;
            set
            {
                if (_state == value) return;
                _state = value;

                HandleStateChange();
            }
        }

        public void ChangeStateToInactive() => State = ARM_LINE_STATE.Inactive;
        public void ChangeStateToGrowing() => State = ARM_LINE_STATE.Growing;
        public void ChangeStateToSnap() => State = ARM_LINE_STATE.Snap;
        public void ChangeStateToRetract() => State = ARM_LINE_STATE.Retract;

        public Vector3 GetHandSegmentDirection()
        {
            if (_pointPositions.Count < 2) return Vector3.up;

            Vector3 dir = _pointPositions[1] - _pointPositions[0];
            return dir.sqrMagnitude < 0.0001f ? Vector3.up : dir.normalized;
        }

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();

            if (_anchor == null || _target == null)
            {
                Debug.LogError($"{nameof(NewArmLineRenderer)} on {name} is missing _anchor or _target.", this);
                enabled = false;
                return;
            }
            ResetChain();
        }

        private void Start()
        {
            HandleStateChange();
        }

        private void Update()
        {
            if (_state == ARM_LINE_STATE.Inactive) return;

            PinFirstAndLastPoints();

            switch (_state)
            {
                case ARM_LINE_STATE.Growing:
                    UpdateGrowing();
                    UpdatePositionMovement();
                    break;
            }

            ApplyPositionsToLineRenderer();
        }

        private void OnDestroy()
        {
            KillSnapSequence();
        }

        public event Action OnSnapToLineStarted;
        public event Action OnSnapToLineEnded;

        private void SnapToLine(int firstN)
        {
            KillSnapSequence();

            Vector3 anchorPos = _anchor.position;
            Vector3 targetPos = _target.position;
            int lastIndex = _pointPositions.Count - 1;

            // Don't include the pinned endpoints
            int count = Mathf.Clamp(firstN, 0, lastIndex - 1);

            _snapSequence = DOTween.Sequence();

            for (int i = 1; i <= count; i++)
            {
                float t = (float)i / lastIndex;
                Vector3 straightLinePoint = Vector3.Lerp(targetPos, anchorPos, t);

                int index = i;
                Tween tween = DOTween.To(
                    () => _pointPositions[index],
                    value => _pointPositions[index] = value,
                    straightLinePoint,
                    _snapDuration
                ).SetEase(Ease.OutElastic);

                _snapSequence.Join(tween);
            }

            _snapSequence.OnComplete(() =>
            {
                _snapSequence = null;

                State = ARM_LINE_STATE.Retract;
                OnSnapToLineEnded?.Invoke();
            });

            OnSnapToLineStarted?.Invoke();
        }

        private void KillSnapSequence()
        {
            _snapSequence?.Kill();
            _snapSequence = null;
        }

        private void ResetChain()
        {
            _pointPositions.Clear();
            _pointVelocities.Clear();

            _pointPositions.Add(_target.position); // index 0: target
            _pointPositions.Add(_anchor.position); // last index: anchor
            _pointVelocities.Add(Vector3.zero);
            _pointVelocities.Add(Vector3.zero);

            _growIndex = 0;
        }

        private void UpdateGrowing()
        {
            int targetIndex = _pointPositions.Count - 1;

            while (Vector3.Distance(_pointPositions[_growIndex], _pointPositions[targetIndex]) > _maxDistance)
            {
                Vector3 frontierPoint = _pointPositions[_growIndex];
                Vector3 targetPoint = _pointPositions[targetIndex];

                Vector3 direction = (targetPoint - frontierPoint).normalized;
                Vector3 newPoint = frontierPoint + direction * _targetDistance;

                int insertIndex = _growIndex + 1;
                _pointPositions.Insert(insertIndex, newPoint);
                _pointVelocities.Insert(insertIndex, Vector3.zero);

                _growIndex = insertIndex;
                targetIndex++; // shifted right by the insert
            }
        }

        private void PinFirstAndLastPoints()
        {
            // keep the two ends pinned every frame
            _pointPositions[0] = _target.position;
            _pointPositions[_pointPositions.Count - 1] = _anchor.position;
        }

        private void UpdatePositionMovement()
        {
            for (int i = 1; i < _pointPositions.Count - 1; i++)
            {
                Vector3 previous = _pointPositions[i - 1];
                Vector3 next = _pointPositions[i + 1];

                Vector3 direction = (next - previous).normalized;

                // rotate the direction itself by a wiggling angle so it undulates like a snake
                float wiggleAngle = Mathf.Sin(Time.time * _wiggleSpeed + i) * _wiggleMagnitude;
                Vector3 wiggledDirection = Quaternion.AngleAxis(wiggleAngle, Vector3.forward) * direction;

                Vector3 desiredPosition = previous + wiggledDirection * _targetDistance;

                Vector3 velocity = _pointVelocities[i];
                float smoothTime = Mathf.Max(_smoothSpeed, 0.0001f) + i / Mathf.Max(_trailSpeed, 0.0001f);

                _pointPositions[i] = Vector3.SmoothDamp(
                    _pointPositions[i],
                    desiredPosition,
                    ref velocity,
                    smoothTime
                );

                _pointVelocities[i] = velocity;
            }
        }

        private void ApplyPositionsToLineRenderer()
        {
            _lineRenderer.positionCount = _pointPositions.Count;
            for (int i = 0; i < _pointPositions.Count; i++)
            {
                _lineRenderer.SetPosition(i, _pointPositions[i]);
            }
        }

        private void RetractToLine()
        {
            Vector3 anchorPos = _anchor.position;
            Vector3 targetPos = _target.position;

            _pointPositions.Clear();
            _pointVelocities.Clear();

            _pointPositions.Add(targetPos);
            _pointPositions.Add(anchorPos);
            _pointVelocities.Add(Vector3.zero);
            _pointVelocities.Add(Vector3.zero);

            _growIndex = 0;
        }

        private void HandleStateChange()
        {
            switch (_state)
            {
                case ARM_LINE_STATE.Inactive:
                    if (_lineRenderer.enabled) _lineRenderer.enabled = false;
                    break;

                case ARM_LINE_STATE.Growing:
                    ResetChain();
                    if (!_lineRenderer.enabled) _lineRenderer.enabled = true;
                    break;

                case ARM_LINE_STATE.Snap:
                    if (!_lineRenderer.enabled) _lineRenderer.enabled = true;
                    SnapToLine(40); // snap to a straight line
                    break;

                case ARM_LINE_STATE.Retract:
                    if (!_lineRenderer.enabled) _lineRenderer.enabled = true;
                    RetractToLine();
                    break;
            }
        }
    }
}