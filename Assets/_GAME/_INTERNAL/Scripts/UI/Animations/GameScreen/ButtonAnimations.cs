using DG.Tweening;
using System;
using UnityEngine;

namespace UI.Animations.GameScreen
{
    [Serializable]
    public class ButtonAnimations
    {
        [Header("Click Animations Setup")]
        [SerializeField] private float _clickAnimationDuration = 0.25f;
        [SerializeField] private Vector2 _clickedScale = Vector2.one;
        [SerializeField] private float _clickRandomRotateOffset = 10f;
        [SerializeField] private float _clickRotateAnimationDuration = 0.15f;
        [SerializeField] private Ease _inRotationEase = Ease.OutBack;
        [SerializeField] private Ease _outRotationEase = Ease.InBack;
        [SerializeField] private bool _useRandomRotationOnClick = false;

        [Space(5), Header("Hover Animations Setup")]
        [SerializeField] private bool _usingHoverAnimation = false;
        [SerializeField] private float _hoverAnimationDuration = 1.25f;
        [SerializeField] private Vector2 _hoverOffset = Vector2.zero;
        [SerializeField] private LoopType _hoverLoopType = LoopType.Yoyo;
        [SerializeField] private Ease _hoverEase = Ease.InOutSine;
        [SerializeField, Tooltip("Use -1 for cycling animation")] private int _hoverLoopsCount = -1;

        [Space(5), Header("Pulse Animation Setup")]
        [SerializeField] private bool _usingPulseAnimation = false;
        [SerializeField] private float _pulseAnimationDuration = 1.0f;
        [SerializeField] private Vector2 _pulseScaleFactor = Vector2.one;
        [SerializeField] private LoopType _pulseLoopType = LoopType.Yoyo;
        [SerializeField] private Ease _pulseEase = Ease.InOutSine;
        [SerializeField, Tooltip("Use -1 for cycling animation")] private int _pulseLoopsCount = -1;

        private RectTransform _rectTransform;

        private Vector2 _originalScale;
        private Vector2 _pulseTargetScale;

        private Tween _pulseTween;
        private Tween _hoverTween;
        private Tween _clickUpTween;
        private Tween _clickDownTween;

        private Sequence _clickSequence;

        public bool Initialized => _rectTransform != null;

        public void Init(RectTransform target)
        {
            _rectTransform = target;
            _originalScale = _rectTransform.localScale;

            _clickAnimationDuration = 0.05f;

            _pulseTargetScale = _originalScale * _pulseScaleFactor;

            if (_usingPulseAnimation)
                PulseAnimation(_pulseLoopsCount, _pulseLoopType);

            if (_usingHoverAnimation)
                HoverAnimation();
        }

        public void ClickDownAnimation()
        {
            _clickUpTween?.Kill();
            _clickDownTween?.Kill();

            _clickDownTween = _rectTransform
                .DOScale(_clickedScale, _clickAnimationDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => _rectTransform.localScale = _clickedScale);
        }

        public void ClickUpAnimation()
        {
            _clickDownTween?.Kill();
            _clickUpTween?.Kill();

            _clickUpTween = _rectTransform
                .DOScale(Vector2.one, _clickAnimationDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => _rectTransform.localScale = Vector2.one);
        }

        public void StopAnimations() => _rectTransform.DOKill();

        public void StopPulseAnimation() => _pulseTween?.Kill(true);

        public void StopHoverAnimation() => _hoverTween?.Kill(true);

        public void HoverAnimation()
        {
            _hoverTween?.Kill();

            Vector2 hoverTargetPosition = _rectTransform.anchoredPosition + _hoverOffset;

            _hoverTween = _rectTransform
                .DOAnchorPos(hoverTargetPosition, _hoverAnimationDuration)
                .SetEase(_hoverEase)
                .SetLoops(_hoverLoopsCount, _hoverLoopType);
        }

        public void PulseAnimation(int loops = -1, LoopType loopType = LoopType.Yoyo)
        {
            _pulseTween?.Kill();

            _pulseTween = _rectTransform
                .DOScale(_pulseTargetScale, _pulseAnimationDuration)
                .SetEase(_pulseEase)
                .SetLoops(loops, loopType);
        }

        public void ButtonClickAnimation(Action onComplete = null)
        {
            _clickSequence?.Kill();

            float randomZRotationOffset = UnityEngine.Random.Range(-_clickRandomRotateOffset, _clickRandomRotateOffset);
            float pressRotationDuration = _clickRotateAnimationDuration;
            float releaseRotationDuration = _clickRotateAnimationDuration * 0.5f;

            _clickSequence = DOTween.Sequence();

            _clickSequence.Append(
                _rectTransform
                .DOScale(_clickedScale, _clickAnimationDuration)
                .SetEase(Ease.OutSine));

            if (_useRandomRotationOnClick)
            {
                _clickSequence.Join(
                _rectTransform
                .DOLocalRotate(new(0f, 0f, randomZRotationOffset), pressRotationDuration))
                .SetEase(_inRotationEase);
            }

            _clickSequence.Append(_rectTransform
                .DOScale(Vector2.one, _clickAnimationDuration)
                .SetEase(Ease.InSine));

            if (_useRandomRotationOnClick)
            {
                _clickSequence.Join(
                _rectTransform
                .DOLocalRotate(new(0f, 0f, 0f), releaseRotationDuration))
                .SetEase(_outRotationEase);
            }

            _clickSequence.OnComplete(() =>
            {
                _rectTransform.localScale = _originalScale;
                _rectTransform.localRotation = Quaternion.identity;
                onComplete?.Invoke();
            });
        }

        public Tween GetWaveTween(Vector2 targetScale, float duration)
        {
            _pulseTween?.Pause();
            _hoverTween?.Pause();

            _rectTransform.localScale = _originalScale;

            return _rectTransform
                .DOScale(targetScale, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    _pulseTween?.Play();
                    _hoverTween?.Play();
                });
        }
    }
}