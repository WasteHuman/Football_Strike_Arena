using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UI.Animations.Game;
using UnityEngine;

namespace UI.Other
{
    public class GameplayTutorialView : ObjectAnimations
    {
        [Header("View Refs")]
        [SerializeField] private TextMeshProUGUI _messageLabel;
        [SerializeField] private RectTransform _dragCircleTransform;
        [SerializeField] private ActionButton _skipTutorialButton;

        [Space(5), Header("Drag Circle Animation Setup")]
        [SerializeField] private float _moveAnimationDuration = 0.55f;
        [SerializeField] private float _animationRepeatDelay = 0.25f;
        [SerializeField] private float _moveOffsetX = 10f;
        [SerializeField] private Ease _moveInEase = Ease.OutQuad;
        [SerializeField] private Ease _moveOutEase = Ease.InQuad;

        private Sequence _circleMoveSequence;

        public event Action OnTutorialSkipped;

        void OnDestroy()
        {
            _circleMoveSequence?.Kill();
            _skipTutorialButton.OnButtonClick -= HandleSkipTutorialButtonClick;
        }

        public void Initialize()
        {
            if(!gameObject.activeSelf)
                gameObject.SetActive(true);

            _skipTutorialButton.OnButtonClick += HandleSkipTutorialButtonClick;
            AsyncLoopAnimation(this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void Hide(Action onComplete = null)
        {
            FadeAppearAnimation(0f, () =>
            {
                onComplete?.Invoke();
                gameObject.SetActive(false);
            });
        }

        private async UniTask AsyncLoopAnimation(CancellationToken token)
        {
            float minX = -_moveOffsetX;
            float maxX = _moveOffsetX;

            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_animationRepeatDelay));

                _circleMoveSequence?.Kill();

                _circleMoveSequence = DOTween.Sequence();

                _circleMoveSequence
                    .Append(_dragCircleTransform.DOAnchorPosX(minX, _moveAnimationDuration)
                    .SetEase(_moveInEase));
                _circleMoveSequence
                    .Append(_dragCircleTransform.DOAnchorPosX(0f, _moveAnimationDuration * 0.75f)
                    .SetEase(_moveOutEase));
                _circleMoveSequence
                    .Append(_dragCircleTransform.DOAnchorPosX(maxX, _moveAnimationDuration)
                    .SetEase(_moveInEase));
                _circleMoveSequence
                    .Append(_dragCircleTransform.DOAnchorPosX(0f, _moveAnimationDuration * 0.75f)
                    .SetEase(_moveOutEase));

                await _circleMoveSequence.AsyncWaitForCompletion();
            }
        }

        private void HandleSkipTutorialButtonClick() => OnTutorialSkipped?.Invoke();
    }
}