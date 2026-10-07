using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Animations.Game
{
    public abstract class ObjectAnimations : MonoBehaviour
    {
        [Header("Animations Duration Setup")]
        [SerializeField] private float _scaleAppearAnimationDuration = 0.35f;
        [SerializeField] private float _moveAppearAnimationDuration = 1.75f;
        [SerializeField] private float _fadeAppearAnimtionDuration = 1.25f;
        [SerializeField] private float _hoverAnimationDuration = 0.5f;
        [SerializeField] private float _pulseAnimationDuration = 1f;
        [SerializeField] private float _collaplseAnimationDuration = 0.55f;
        [SerializeField] private float _shakeScaleAnimationDuration = 0.15f;

        [Space(5), Header("Object Refs")]
        [SerializeField] private RectTransform _objectRect;

        [Space(5), Header("Hover Animation Setup")]
        [SerializeField] private float _yMoveOffset = 1.0f;
        [SerializeField] private LoopType _hoverLoopType = LoopType.Yoyo;
        [SerializeField, Tooltip("Set -1 for infinite loops count")] private int _hoverLoopCount = -1;

        [Space(5), Header("Move Appear Animation Setup")]
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private Ease _moveAppearInEase = Ease.OutBack;
        [SerializeField] private Ease _moveAppearOutEase = Ease.InBack;

        [Space(5), Header("Fade Appear Animation Setup")]
        [SerializeField] private CanvasGroup _targetCanvasGroup;

        [Space(5), Header("Collapse Animation Setup")]
        [SerializeField] private float _shakeScaleStrength = 0.08f;
        [SerializeField] private Ease _collapseInEase = Ease.OutQuad;
        [SerializeField] private Ease _collapseOutEase = Ease.InBack;

        [Space(5), Header("Pulse Animation Setup")]
        [SerializeField] private float _pulseTargetScale = 0.9f;
        [SerializeField] private LoopType _pulseLoopType = LoopType.Yoyo;
        [SerializeField, Tooltip("Set -1 for infinite loops count")] private int _pulseLoopsCount = -1;

        [Space(5), Header("Animations Flags")]
        [SerializeField] private bool _hoverAnimationEnabled = false;
        [SerializeField] private bool _pulseAnimationEnabled = false;

        protected Vector3 _originalScale;
        protected Vector3 _originalPosition;

        private Tween _scaleAppearTween;
        private Tween _fadeAppearTween;
        private Tween _moveAppearTween;
        private Tween _scaleDisappearTween;
        private Tween _hoverTween;
        private Tween _pulseTween;
        private Tween _shakeScaleTween;
        private Sequence _collapseSequence;

        private void OnEnable()
        {
            if (_hoverAnimationEnabled)
                HoverAnimation();

            if (_pulseAnimationEnabled)
                PulseAnimation();
        }

        private void OnDisable()
        {
            _hoverTween?.Kill();
            _scaleAppearTween?.Kill();
            _scaleDisappearTween?.Kill();
            _pulseTween?.Kill();
            _moveAppearTween?.Kill();
            _fadeAppearTween?.Kill();
        }

        private void OnDestroy()
        {
            _hoverTween?.Kill();
            _scaleAppearTween?.Kill();
            _scaleDisappearTween?.Kill();
            _pulseTween?.Kill();
            _moveAppearTween?.Kill();
            _fadeAppearTween?.Kill();
        }

        private void HoverAnimation()
        {
            _hoverTween?.Kill();

            Vector3 originalPosition = transform.localPosition;
            float targetY = transform.localPosition.y + _yMoveOffset;

            _hoverTween = transform
                .DOLocalMoveY(targetY, _hoverAnimationDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(_hoverLoopCount, _hoverLoopType)
                .OnKill(() => transform.localPosition = originalPosition);
        }

        private void PulseAnimation()
        {
            _pulseTween?.Kill();

            _originalScale = transform.localScale;

            _pulseTween = transform
                .DOScale(_pulseTargetScale, _pulseAnimationDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(_pulseLoopsCount, _pulseLoopType);
        }

        public void ScaleAppear(Vector3 originalScale, Action onComplete = null)
        {
            transform.localScale = Vector3.zero;
            gameObject.SetActive(true);
            _originalScale = originalScale;

            _scaleAppearTween?.Kill();
            _scaleDisappearTween?.Kill();

            _scaleAppearTween = transform
                .DOScale(originalScale, _scaleAppearAnimationDuration)
                .SetEase(Ease.InOutBounce)
                .OnComplete(() =>
                {
                    transform.localScale = _originalScale;
                    onComplete?.Invoke();
                });
        }

        public void ScaleDisappear(Action onComplete = null)
        {
            _scaleAppearTween?.Kill();
            _scaleDisappearTween?.Kill();

            float disappearAnimationDuration = _scaleAppearAnimationDuration * 0.25f;

            _scaleDisappearTween = transform
                .DOScale(Vector3.zero, disappearAnimationDuration)
                .SetEase(Ease.InOutBounce)
                .OnComplete(() =>
                {
                    transform.localScale = Vector3.zero;
                    onComplete?.Invoke();
                })
                .OnKill(() => transform.localScale = _originalScale);
        }

        public void CollapseAnimation(Action onComplete = null)
        {
            _collapseSequence?.Kill();
            _shakeScaleTween?.Kill();

            transform.localScale = _originalScale;

            _collapseSequence = DOTween.Sequence();

            float squashDuration = _collaplseAnimationDuration * 0.15f;
            float collapseDuration = _collaplseAnimationDuration * 0.75f;

            _shakeScaleTween = transform.DOShakeScale(_shakeScaleStrength, _shakeScaleAnimationDuration);

            _collapseSequence
                .Append(transform.DOScale(Vector3.zero, squashDuration))
                .SetEase(_collapseOutEase);

            _collapseSequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                transform.localScale = _originalScale;
                onComplete?.Invoke();
            });
        }

        public void MoveAppearAnimation(bool appear = true, Action onComplete = null)
        {
            if(_objectRect == null)
                return;

            _moveAppearTween?.Kill();

            if (appear)
            {
                _moveAppearTween = _objectRect
                .DOAnchorPos(_targetPosition, _moveAppearAnimationDuration)
                .SetEase(_moveAppearInEase);
            }
            else
            {
                _moveAppearTween = _objectRect
                .DOAnchorPos(_originalPosition, _moveAppearAnimationDuration)
                .SetEase(_moveAppearOutEase);
            }

            _moveAppearTween.OnComplete(() =>
            {
                if(appear)
                    _objectRect.transform.position = _targetPosition;
                else
                    _objectRect.transform.position = _originalPosition;

                onComplete?.Invoke();
            });
        }

        public void FadeAppearAnimation(float targetAplha = 1f, Action onCompleted = null)
        {
            if(!_targetCanvasGroup.gameObject.activeSelf)
                _targetCanvasGroup.gameObject.SetActive(true);

            _fadeAppearTween?.Kill();

            _fadeAppearTween = _targetCanvasGroup
                .DOFade(targetAplha, _fadeAppearAnimtionDuration)
                .OnComplete(() =>
                {
                    _targetCanvasGroup.alpha = targetAplha;
                    onCompleted?.Invoke();
                });
        }
    }
}