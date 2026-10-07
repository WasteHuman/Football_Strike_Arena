using System;
using System.Collections.Generic;
using System.Threading;
using Core.Data;
using Core.Gameplay.Game.TargetSystem;
using Core.Services;
using Cysharp.Threading.Tasks;
using Extensions.GameObject;
using UI.Other;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Gameplay.Game.Player
{
    public class PlayerController : MonoBehaviour
    {
        private const float MIN_X = -3.2f;
        private const float MAX_X = 3.2f;

        [Header("Player Setup")]
        [SerializeField] private GameObject _player;
        [SerializeField] private SpriteRenderer _playerView;
        [SerializeField] private float _playerMoveSpeed = 5f;
        [SerializeField] private float _playerReloadTime = 2f;
        [SerializeField] private float _playerShootForce = 5f;
        [Tooltip("Using for setup delay between Player Pose sprites")]
        [SerializeField] private float _playerSpriteChangeDelay = 0.35f;

        [Space(5), Header("Fire Vision Assist Setup")]
        [Tooltip("Fire vision assist strength from 0 to 100 percent")]
        [Range(0f, 100f)]
        [SerializeField] private float _fireVisionAssistStrength = 0f;
        [SerializeField] private TargetBallSpawner _ballSpawner;

        [Space(5), Header("Player Pose Sprites Setup")]
        [SerializeField] private List<PlayerSkinData> _playerSkinDatas = new();

        [Space(5), Header("Ball Projectile Setup")]
        [SerializeField] private PlayerProjectile _ballProjectile;
        [SerializeField] private Transform _ballProjectileContainer;
        [SerializeField] private List<PlayerBallSkinData> _playerBallSkinDatas = new();

        private PlayerState _state = PlayerState.Idle;
        private PlayerSkinData _playerSkinData;
        private Sprite _currentBallSkin;
        private int _lastMoveDirection = 1; // 1 = right, -1 = left
        private TargetBallView _nearestTarget;
        private Camera _camera;

        private UniTaskCompletionSource _hitProjectileSource;
        private bool _isPlayerAlive = true;
        private bool _isGameplayStarted;

        public bool IsPlayerAlive => _isPlayerAlive;

        public event Action OnPlayerLose;

        public void Initialize()
        {
            _isPlayerAlive = true;
            _isGameplayStarted = false;

            LoadCurrentPlayerSkins(
                GameServices.PlayerService.CurrentPlayerSkinId,
                GameServices.PlayerService.CurrentPlayerBallSkinId);

            _ballProjectile.Init(_ballProjectileContainer, _currentBallSkin, GameServices.PlayerService.CurrentPlayerDamage);
            _playerReloadTime = GameServices.PlayerService.CurrentPlayerReload;
            _ballProjectile.OnBallHitted += HandleHittedBall;

            _isPlayerAlive = true;

            _camera = Camera.main;

            ProjectileFlowAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void Dispose()
        {
            _ballProjectile.OnBallHitted -= HandleHittedBall;

            OnPlayerLose = null;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if(!_isPlayerAlive)
                return;
            
            if(!collision.gameObject.GetComponentOrNull<TargetBallView>())
                return;

            _isPlayerAlive = false;
            OnPlayerLose?.Invoke();
        }

        void Update()
        {
            if(!_isPlayerAlive || !_isGameplayStarted)
                return;

                if (_fireVisionAssistStrength > 0f)
                    _nearestTarget = FindNearestTarget();

            if (TryGetDragTargetPosition(out var dragWorldPosition, out var isDragging))
            {
                if (!isDragging)
                    return;

                var clampedX = Mathf.Clamp(dragWorldPosition.x, MIN_X, MAX_X);
                var targetPosition = new Vector3(clampedX, _player.transform.position.y, _player.transform.position.z);
                _player.transform.position = Vector3.Lerp(_player.transform.position, targetPosition, Time.deltaTime * _playerMoveSpeed);

                if (dragWorldPosition.x > _player.transform.position.x + 0.01f)
                    _lastMoveDirection = 1;
                else if (dragWorldPosition.x < _player.transform.position.x - 0.01f)
                    _lastMoveDirection = -1;

                _player.transform.localScale = 
                    new Vector3(_lastMoveDirection * Mathf.Abs(_player.transform.localScale.x), _player.transform.localScale.y, _player.transform.localScale.z);

                return;
            }

#if UNITY_EDITOR
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                HandleLeftButtonClick();

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                HandleRightButtonClick();
#endif
        }

        private void LoadCurrentPlayerSkins(string currentPlayerSkinId, string currentPlayerBallSkinId)
        {
            if (_playerSkinDatas == null || _playerSkinDatas.Count == 0)
            {
                Debug.LogWarning("[PlayerController] Player skin list is empty. Using empty skin data fallback.");
                _playerSkinData = new PlayerSkinData();
                return;
            }

            var playerSkin = _playerSkinDatas.Find(skinData => skinData != null && skinData.SkinId == currentPlayerSkinId);

            if (playerSkin == null)
            {
                Debug.LogWarning($"[PlayerController] Skin id [{currentPlayerSkinId}] not found. Using first available skin.");
                playerSkin = _playerSkinDatas[0];
            }

            _playerSkinData = playerSkin;
            _playerView.sprite = playerSkin.IdlePose;

            if (_playerBallSkinDatas == null || _playerBallSkinDatas.Count == 0)
            {
                Debug.LogWarning("[PlayerController] Ball skin list is empty. Leaving player ball skin unset.");
                _currentBallSkin = null;
                return;
            }

            var playerBallSkin = _playerBallSkinDatas.Find(skinData => skinData != null && skinData.SkinId == currentPlayerBallSkinId);

            if (playerBallSkin == null)
            {
                Debug.LogWarning($"[PlayerController] Ball skin id [{currentPlayerBallSkinId}] not found. Using first available ball skin.");
                playerBallSkin = _playerBallSkinDatas[0];
            }

            _currentBallSkin = playerBallSkin?.Skin;
        }

        private async UniTask ProjectileFlowAsync(CancellationToken token)
        {
            while (_isPlayerAlive && !token.IsCancellationRequested)
            {
                if (_state != PlayerState.Idle || !_isGameplayStarted)
                {
                    await UniTask.Yield(token);
                    continue;
                }

                await PlayerHitProcessAsync(token);

                _hitProjectileSource = new();
                _ballProjectile.ShootProjectile(_playerShootForce);

                await UniTask.Delay(TimeSpan.FromSeconds(_playerSpriteChangeDelay * 0.5f), cancellationToken: token);

                _playerView.sprite = _playerSkinData.IdlePose;

                await ProcessProjectileHitAsync(token);

                await ProjectileReloadAsync(token);
            }
        }

        private async UniTask ProjectileReloadAsync(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_playerReloadTime), cancellationToken: token);

            _state = PlayerState.Idle;

            _ballProjectile.Show();

            return;
        }

        private async UniTask PlayerHitProcessAsync(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_playerSpriteChangeDelay), cancellationToken: token);
            _playerView.sprite = _playerSkinData.HitPreparePose;
            await UniTask.Delay(TimeSpan.FromSeconds(_playerSpriteChangeDelay), cancellationToken: token);
            _playerView.sprite = _playerSkinData.HitPose;

            return;
        }

        private async UniTask ProcessProjectileHitAsync(CancellationToken token)
        {
            await _hitProjectileSource.Task.AttachExternalCancellation(token);

            _ballProjectile.Hide();
            await UniTask.Delay(10, cancellationToken: token);

            _ballProjectile.ResetProjectilePosition();
            await UniTask.Delay(10, cancellationToken: token);

            _state = PlayerState.Reload;
            return;
        }

        public void StartGameplay()
        {
            if (_isGameplayStarted || !_isPlayerAlive)
                return;

            _isGameplayStarted = true;
        }

        public float GetFireVisionAssistStrength() => _fireVisionAssistStrength / 100f;

        public TargetBallView GetNearestTarget() => _nearestTarget;

        private bool TryGetDragTargetPosition(out Vector3 worldPosition, out bool isDragging)
        {
            worldPosition = Vector3.zero;
            isDragging = false;

            if (Camera.main == null)
                return false;

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                var touch = Touchscreen.current.primaryTouch;
                var delta = touch.delta.ReadValue();
                if (delta.sqrMagnitude <= 0.01f)
                    return false;

                isDragging = true;
                var screenPosition = touch.position.ReadValue();
                worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                var delta = Mouse.current.delta.ReadValue();
                if (delta.sqrMagnitude <= 0.01f)
                    return false;

                isDragging = true;
                var screenPosition = Mouse.current.position.ReadValue();
                worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
                return true;
            }

            return false;
        }

        private TargetBallView FindNearestTarget()
        {
            var allTargets = _ballSpawner.GetActiveTargets();

            if (allTargets == null || allTargets.Count == 0)
                return null;

            TargetBallView nearest = null;
            float nearestDistance = float.MaxValue;
            Vector2 playerPosition = _player.transform.position;
            Vector2 playerDirection = new(_lastMoveDirection, 0);

            for (int i = 0; i < allTargets.Count; i++)
            {
                if (allTargets[i] == null || !allTargets[i].gameObject.activeSelf)
                    continue;

                if (!IsTargetVisibleOnScreen(allTargets[i]))
                    continue;

                Vector2 toTarget = ((Vector2)allTargets[i].transform.position - playerPosition).normalized;
                float dotProduct = Vector2.Dot(playerDirection, toTarget);

                if (dotProduct <= 0f)
                    continue;

                float distance = Vector2.Distance(playerPosition, allTargets[i].transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = allTargets[i];
                }
            }

            return nearest;
        }

        private bool IsTargetVisibleOnScreen(TargetBallView target)
        {
            if (_camera == null)
                return false;

            Vector3 viewportPoint = _camera.WorldToViewportPoint(target.transform.position);

            return viewportPoint.z > 0 &&
                   viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
                   viewportPoint.y >= 0f && viewportPoint.y <= 1f;
        }

        private void HandleLeftButtonClick()
        {
            if(!_isPlayerAlive)
                return;

            _lastMoveDirection = -1;

            var nextX = _player.transform.position.x - _playerMoveSpeed * Time.deltaTime;
            _player.transform.position = new Vector3(
                Mathf.Max(nextX, MIN_X),
                _player.transform.position.y,
                _player.transform.position.z
            );

            _player.transform.localScale = 
                new Vector3(_lastMoveDirection * Mathf.Abs(_player.transform.localScale.x), _player.transform.localScale.y, _player.transform.localScale.z);
        }

        private void HandleRightButtonClick()
        {
            if(!_isPlayerAlive)
                return;

            _lastMoveDirection = 1;

            var nextX = _player.transform.position.x + _playerMoveSpeed * Time.deltaTime;
            _player.transform.position = new Vector3(
                Mathf.Min(nextX, MAX_X),
                _player.transform.position.y,
                _player.transform.position.z
            );

            _player.transform.localScale = 
                new Vector3(_lastMoveDirection * Mathf.Abs(_player.transform.localScale.x), _player.transform.localScale.y, _player.transform.localScale.z);
        }

        private void HandleHittedBall() => _hitProjectileSource?.TrySetResult();
    }
}