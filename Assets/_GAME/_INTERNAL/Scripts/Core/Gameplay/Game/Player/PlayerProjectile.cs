using System;
using Core.Gameplay.Game.TargetSystem;
using Core.Services.Audio;
using Extensions.GameObject;
using UI.Animations.Game;
using UnityEngine;

namespace Core.Gameplay.Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class PlayerProjectile : ObjectAnimations
    {
        [SerializeField] private Vector2 _shootDirection = Vector2.up;
        [Tooltip("Reference to player controller for fire vision assist")]
        [SerializeField] private PlayerController _playerController;

        private int _currentDamage = 1;

        private Rigidbody2D _rb;
        private Transform _defaultPosition;
        private SpriteRenderer _spriteRenderer;
        private bool _hasHit;

        public int CurrentDamage => _currentDamage;

        public event Action OnBallHitted;

        void OnCollisionEnter2D(Collision2D collision)
        {
            if(!collision.gameObject.GetComponentOrNull<TargetBallView>())
                return;
            
            HitBall();
        }

        void OnDestroy()
        {
            OnBallHitted = null;
        }

        public void Init(Transform defaultPosition, Sprite ballSkin, int currentDamage = 1)
        {
            _originalScale = transform.localScale;
            _defaultPosition = defaultPosition;
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _spriteRenderer.sprite = ballSkin;

            _currentDamage = currentDamage;
        }

        public void BallAtTheKillzone() => HitBall();
        public void Show() => ScaleAppear(_originalScale);
        public void Hide() => ScaleDisappear(() => gameObject.SetActive(false));

        public void ShootProjectile(float shootForce)
        {
            _hasHit = false;

            Vector2 finalShootDirection = _shootDirection;
            if (_playerController != null && _playerController.GetFireVisionAssistStrength() > 0f)
            {
                var nearestTarget = _playerController.GetNearestTarget();
                if (nearestTarget != null)
                {
                    float assistStrength = _playerController.GetFireVisionAssistStrength();
                    Vector2 targetDirection = ((Vector2)nearestTarget.transform.position - (Vector2)_defaultPosition.position).normalized;
                    finalShootDirection = Vector2.Lerp(_shootDirection, targetDirection, assistStrength).normalized;
                }
            }

            _rb.angularVelocity = 0f;
            _rb.linearVelocity = Vector2.zero;

            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.AddForce(finalShootDirection * shootForce, ForceMode2D.Impulse);
            AudioService.Instance.PlaySfx(SoundType.Player_Ball_Shoot);
            transform.SetParent(null);
        }

        public void ResetProjectilePosition()
        {
            _rb.angularVelocity = 0f;
            _rb.linearVelocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Kinematic;

            transform.SetParent(_defaultPosition);
            transform.position = _defaultPosition.position;
        }

        private void HitBall()
        {
            if(_hasHit)
                return;
                
            _hasHit = true;
            AudioService.Instance.PlaySfx(SoundType.Player_Ball_Hit);

            OnBallHitted?.Invoke();
            _rb.angularVelocity = 0f;
            _rb.linearVelocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}