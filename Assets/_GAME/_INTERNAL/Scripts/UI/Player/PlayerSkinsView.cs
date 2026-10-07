using System;
using System.Threading;
using Core.Extensions.RectTransform;
using Core.Services;
using Core.SO;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Player
{
    public class PlayerSkinsView : MonoBehaviour
    {
        [SerializeField] private Image _playerSkin;
        [SerializeField] private Image _playerBallSkin;

        [Space(5), Header("Skins Change Animations Setup")]
        [SerializeField] private bool _useBallChangeAnimation = false;
        [SerializeField] private bool _usePlayerSkinChangeAnimation = false;
        [SerializeField] private Vector3 _ballTargetPosition = new(415f, 50f, 0f);
        [SerializeField] private float _ballChangeAnimationDuration = 0.6f;
        
        [Header("Ball Animation Effects")]
        [SerializeField] private Ease _ballRollEase = Ease.InOutQuad;
        [SerializeField] private Ease _ballRotateEase = Ease.InCubic;
        [SerializeField] private float _ballBlickAnimationDuration = 0.55f;
        [SerializeField] private float _ballBlickAnimationDelay = 1.25f;
        [SerializeField] private RectTransform _blickRect;
        [SerializeField] private Ease _ballBlickEase = Ease.InOutCubic;
        [SerializeField] private Vector3 _blickTargetPosition;

        [Header("Player Skin Animation Effects")]
        [SerializeField] private float _playerSkinFadeOutDuration = 0.3f;
        [SerializeField] private float _playerSkinFadeInDuration = 0.25f;
        [SerializeField] private float _playerSkinScaleDownMin = 0.8f;
        [SerializeField] private Ease _playerSkinFadeOutEase = Ease.InQuad;
        [SerializeField] private Ease _playerSkinScaleDownEase = Ease.OutCubic;
        [SerializeField] private Ease _playerSkinFadeInEase = Ease.OutQuad;
        [SerializeField] private Ease _playerSkinScaleUpEase = Ease.OutCubic;

        private RectTransform _ballRect;
        private RectTransform _playerRect;

        private Vector3 _originalPlayerScale;
        private Vector3 _originalBlickPosition;

        private Sequence _playerSkinChangeSequence;
        private Sequence _ballSkinChangeSequence;
        private Tween _ballBlickTween;

        private ShopEntitiesConfig _spriteConfig;

        void OnDestroy()
        {
            Dispose();
        }

        public void Init()
        {
            _spriteConfig = Resources.Load<ShopEntitiesConfig>("Configs/Shop/ShopEntitiesConfig");

            GameServices.PlayerService.OnPlayerSkinChanged += HandlePlayerSkinChanged;
            GameServices.PlayerService.OnPlayerBallSkinChanged += HandlePlayerBallSkinChanged;

            _playerSkin.sprite = _spriteConfig.GetPlayerSkinById(GameServices.PlayerService.CurrentPlayerSkinId);
            _playerBallSkin.sprite = _spriteConfig.GetPlayerBallSkinById(GameServices.PlayerService.CurrentPlayerBallSkinId);

            _ballRect = _playerBallSkin.gameObject.GetComponent<RectTransform>();
            _playerRect = _playerSkin.gameObject.GetComponent<RectTransform>();

            _originalPlayerScale = _playerRect.localScale;

            if(_blickRect != null)
                _originalBlickPosition = _blickRect.anchoredPosition;

            AsyncBlickAnimation(this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void Dispose()
        {
            GameServices.PlayerService.OnPlayerSkinChanged -= HandlePlayerSkinChanged;
            GameServices.PlayerService.OnPlayerBallSkinChanged -= HandlePlayerBallSkinChanged;

            _ballSkinChangeSequence?.Kill();
            _playerSkinChangeSequence?.Kill();
            _ballBlickTween?.Kill();
        }

        private async UniTask AsyncBlickAnimation(CancellationToken token)
        {
            if(_blickRect == null)
                return;

            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_ballBlickAnimationDelay), cancellationToken: token);

                _blickRect.anchoredPosition = _originalBlickPosition;

                _ballBlickTween = _blickRect
                    .DOAnchorPos(_blickTargetPosition, _ballBlickAnimationDuration)
                    .SetEase(_ballBlickEase)
                    .OnComplete(() => _blickRect.anchoredPosition = _blickTargetPosition);

                await _ballBlickTween.AsyncWaitForCompletion();
            }
        }

        private async UniTask AsyncBallSkinChangeAnimation(string skinId)
        {
            _ballSkinChangeSequence?.Kill(true);

            const float rotationAngle = -360f;

            var sequence = DOTween.Sequence();

            var startPosition = _ballRect.anchoredPosition;
            var startRotation = _ballRect.localEulerAngles;

            var rollRotation = startRotation + Vector3.forward * rotationAngle;

            var moveDuration = _ballChangeAnimationDuration * 0.45f;

            // ─────────────────────────────────────────
            // 1. Перекат вправо
            // ─────────────────────────────────────────

            sequence.Append(
                _ballRect.DOAnchorPos(_ballTargetPosition, moveDuration)
                    .SetEase(_ballRollEase)
            );

            sequence.Join(
                _ballRect.DOLocalRotate(
                        rollRotation,
                        moveDuration,
                        RotateMode.FastBeyond360)
                    .SetEase(_ballRotateEase)
            );

            // ─────────────────────────────────────────
            // 2. Меняем скин
            // ─────────────────────────────────────────

            sequence.AppendCallback(() =>
            {
                _playerBallSkin.sprite =
                    _spriteConfig.GetPlayerBallSkinById(skinId);
            });

            // ─────────────────────────────────────────
            // 3. Возвращаемся обратно
            // ─────────────────────────────────────────

            sequence.Append(
                _ballRect.DOAnchorPos(startPosition, moveDuration)
                    .SetEase(Ease.OutCubic)
            );

            sequence.Join(
                _ballRect.DOLocalRotate(
                        startRotation,
                        moveDuration,
                        RotateMode.FastBeyond360)
                    .SetEase(Ease.OutCubic)
            );

            sequence.OnComplete(() =>
            {
                _ballRect.anchoredPosition = startPosition;
                _ballRect.localRotation = Quaternion.Euler(startRotation);
            });

            _ballSkinChangeSequence = sequence;

            await sequence.AsyncWaitForCompletion();

            return;
        }

        private async UniTask AsyncPlayerSkinChangeAnimation(string skinId)
        {
            _playerSkinChangeSequence?.Kill(true);

            var sequence = DOTween.Sequence();
            var originalColor = _playerSkin.color;

            // Phase 1: Fade out and scale down
            sequence.Append(_playerSkin.DOFade(0f, _playerSkinFadeOutDuration)
                .SetEase(_playerSkinFadeOutEase));
            
            sequence.Join(_playerRect.DOScale(_originalPlayerScale * _playerSkinScaleDownMin, _playerSkinFadeOutDuration)
                .SetEase(_playerSkinScaleDownEase));

            // Change sprite at the peak (when it's faded out)
            sequence.AppendCallback(() => _playerSkin.sprite = _spriteConfig.GetPlayerSkinById(skinId));

            // Phase 2: Fade in and scale up
            sequence.Append(_playerSkin.DOFade(1f, _playerSkinFadeInDuration)
                .SetEase(_playerSkinFadeInEase));
            
            sequence.Join(_playerRect.DOScale(_originalPlayerScale, _playerSkinFadeInDuration)
                .SetEase(_playerSkinScaleUpEase));

            _playerSkinChangeSequence = sequence;
            await sequence.AsyncWaitForCompletion();
        }

        private void HandlePlayerSkinChanged(string id)
        {
            if (_usePlayerSkinChangeAnimation)
            {
                AsyncPlayerSkinChangeAnimation(id).Forget();
            }
            else
            {
                _playerSkin.sprite = _spriteConfig.GetPlayerSkinById(id);
            }
        }

        private void HandlePlayerBallSkinChanged(string id)
        {
            if (_useBallChangeAnimation)
            {
                AsyncBallSkinChangeAnimation(id).Forget();
            }
            else
            {
                _playerBallSkin.sprite = _spriteConfig.GetPlayerBallSkinById(id);
            }
        }
    }
}