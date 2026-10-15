using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UI.Other;

namespace Core.WheelOfLuck
{
    [RequireComponent(typeof(RectTransform))]
    public class WheelView : MonoBehaviour
    {
        [Header("Wheel")]
        [SerializeField] private RectTransform _wheelTransform;
        [SerializeField] private RectTransform _pointer;
        [SerializeField] private List<RewardView> _rewardViews = new();

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI _cooldownLabel;
        [SerializeField] private TextMeshProUGUI _spinsCountLabel;

        [Header("Buttons")]
        [SerializeField] private ActionButton _startSpinButton;
        [SerializeField] private ActionButton _watchAdButton;

        private Vector2 _originalScale;

        private Tween _spinTween;
        private Tween _pulseTween;

        private Tween _switchFromButtonTween;
        private Tween _switchToButtonTween;

        private Action _spinButtonHandler;
        private Action _watchAdButtonHandler;

        public IReadOnlyList<RewardView> RewardViews => _rewardViews;
        public event Action OnSpinClicked;
        public event Action OnWatchAdClicked;

        public void Enter()
        {
            if (_startSpinButton != null)
            {
                _spinButtonHandler = () => OnSpinClicked?.Invoke();
                _startSpinButton.OnButtonClick += _spinButtonHandler;
            }

            if(_watchAdButton != null)
            {
                _watchAdButtonHandler = () => OnWatchAdClicked?.Invoke();
                _watchAdButton.OnButtonClick += _watchAdButtonHandler;
            }

            _originalScale = _wheelTransform.localScale;
        }

        private void OnDestroy()
        {
            _spinTween?.Kill();
            _pulseTween?.Kill();

            if (_startSpinButton != null)
                _startSpinButton.OnButtonClick -= _spinButtonHandler;

            if(_watchAdButton != null)
                _watchAdButton.OnButtonClick -= _watchAdButtonHandler;
        }

        public void SetSpinInteractable(bool interactable)
        {
            if (_startSpinButton == null)
                return;

            if(!_startSpinButton.gameObject.activeSelf || _startSpinButton.transform.localScale == Vector3.zero)
                AnimateButtonSwitch(_watchAdButton, _startSpinButton, interactable);

            _startSpinButton.Interactable = interactable;
            
            if(interactable)
                _cooldownLabel.gameObject.SetActive(false);
        }

        public void SetWatchAdInteractable(bool interactable)
        {
            if(_watchAdButton == null)
                return;

            if(!_watchAdButton.gameObject.activeSelf)
                AnimateButtonSwitch(_startSpinButton, _watchAdButton, interactable);
        }

        private void AnimateButtonSwitch(ActionButton from, ActionButton to, bool interactable)
        {
            if(from == null || to == null)
                return;

            float animationDuration = 0.15f;

            to.transform.localScale = Vector2.zero;

            _switchFromButtonTween?.Kill();

            _switchFromButtonTween = from.transform
                .DOScale(0f, animationDuration)
                .OnComplete(() =>
                {
                    if (!interactable)
                    {
                        from.Animations.StopPulseAnimation();
                        from.Animations.StopHoverAnimation();
                    }

                    from.gameObject.SetActive(!interactable);
                    to.gameObject.SetActive(interactable);

                    _switchToButtonTween?.Kill();

                    _switchToButtonTween = to.transform
                        .DOScale(Vector2.one, animationDuration)
                        .OnComplete(() =>
                        {
                        });
                });
        }

        public void SetFreeSpins(int amount)
        {
            if (_spinsCountLabel != null)
                _spinsCountLabel.text = $"{amount}";
        }

        public void SetCooldown(TimeSpan remaining)
        {
            if (_cooldownLabel == null)
                return;

            if(!_cooldownLabel.gameObject.activeSelf && remaining.Hours > 0f)
                _cooldownLabel.gameObject.SetActive(true);

            _cooldownLabel.text = FormatTimeSpan(remaining);
        }

        public void SetPulse(bool enabled)
        {
            if (_wheelTransform == null)
                return;

            if (!enabled)
            {
                _pulseTween?.Kill();
                _pulseTween = null;
                return;
            }

            if (_pulseTween != null && _pulseTween.IsActive())
                return;

            _wheelTransform.localScale = _originalScale;

            _pulseTween?.Kill();
            _pulseTween = _wheelTransform
                .DOScale(0.95f, 1.75f)
                .SetEase(Ease.OutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(_wheelTransform);
        }

        public void SpinTo(
            int targetIndex,
            float duration,
            int minFullRotations,
            Action onComplete)
        {
            if (_wheelTransform == null)
            {
                Debug.LogWarning("[Wheel] Wheel Transform is not assigned.");
                return;
            }

            if (_pointer == null)
            {
                Debug.LogWarning("[Wheel] Pointer is not assigned.");
                return;
            }

            if (targetIndex < 0 || targetIndex >= _rewardViews.Count)
            {
                Debug.LogWarning($"[Wheel] Invalid target index: {targetIndex}");
                return;
            }

            RewardView targetRewardView = _rewardViews[targetIndex];

            if (!targetRewardView.TryGetComponent<RectTransform>(
                    out RectTransform rewardTransform))
            {
                Debug.LogWarning("[Wheel] RewardView does not have RectTransform.");
                return;
            }

            _spinTween?.Kill();
            SetPulse(false);

            float currentAngle = _wheelTransform.eulerAngles.z;
            float deltaNeeded = CalculateAngleToPerfectAlignment(rewardTransform);

            float targetAbsoluteAngle = currentAngle + deltaNeeded;
            float minRequiredAngle = currentAngle + (minFullRotations * 360f);

            while (targetAbsoluteAngle < minRequiredAngle)
                targetAbsoluteAngle += 360f;

            float endAngle = targetAbsoluteAngle;

            _spinTween = _wheelTransform
                .DORotate(
                    new Vector3(0f, 0f, endAngle),
                    duration,
                    RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    _wheelTransform.eulerAngles =
                        new Vector3(0f, 0f, endAngle);

                    onComplete?.Invoke();
                });
        }

        private float CalculateAngleToPerfectAlignment(
            RectTransform rewardTransform)
        {
            Vector3 wheelCenter = _wheelTransform.position;
            Vector3 rewardWorldPos = rewardTransform.position;
            Vector3 pointerWorldPos = _pointer.position;

            Vector3 toReward = rewardWorldPos - wheelCenter;
            Vector3 toPointer = pointerWorldPos - wheelCenter;

            float angleToReward =
                Mathf.Atan2(toReward.y, toReward.x) * Mathf.Rad2Deg;

            float angleToPointer =
                Mathf.Atan2(toPointer.y, toPointer.x) * Mathf.Rad2Deg;

            return angleToPointer - angleToReward;
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan <= TimeSpan.Zero)
                return "00:00:00";

            int hours = (int)timeSpan.TotalHours;

            return $"{hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}
